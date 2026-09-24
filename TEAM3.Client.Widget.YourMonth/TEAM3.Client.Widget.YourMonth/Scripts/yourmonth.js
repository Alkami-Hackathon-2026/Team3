(function () {
    'use strict';

    var SUMMARY_URL = '/TEAM3YourMonth/Summary';
    var DISMISS_URL = '/TEAM3YourMonth/DismissInsight';
    var SAVE_BUDGETS_URL = '/TEAM3YourMonth/SaveBudgets';

    function text(key) {
        var table = window.team3YourMonthText || {};
        return table[key] || '';
    }

    function money(value) {
        return Alkami.Utils.CurrencyHelper.formatCurrency(value || 0);
    }

    function showAjaxError(error) {
        if (error instanceof Response) {
            error.json().then(function (data) {
                Alkami.FlashBanner.showError(data.errorMessage);
            }).catch(function () {
                Alkami.FlashBanner.showError(text('genericError'));
            });
            return;
        }
        Alkami.FlashBanner.showError(text('genericError'));
    }

    var useYourMonthStore = Pinia.defineStore('yourmonth', {
        state: function () {
            return {
                summary: null,
                loading: true
            };
        },
        getters: {
            percentChangeDisplay: function (state) {
                if (!state.summary || state.summary.PercentChange === null) {
                    return '—';
                }
                var value = state.summary.PercentChange;
                return (value > 0 ? '+' : '') + value + '%';
            }
        },
        actions: {
            loadSummary: function () {
                var self = this;
                return Alkami.Helpers.ajax({
                    url: SUMMARY_URL,
                    responseType: 'json'
                }).then(function (response) {
                    return response.json();
                }).then(function (summary) {
                    self.summary = summary;
                    self.loading = false;
                }).catch(function (error) {
                    self.loading = false;
                    showAjaxError(error);
                });
            },
            saveBudgets: function (budgets, removed, silent) {
                var self = this;
                return Alkami.Helpers.ajax({
                    url: SAVE_BUDGETS_URL,
                    method: 'POST',
                    contentType: 'application/x-www-form-urlencoded',
                    responseType: 'json',
                    data: {
                        budgetsJson: JSON.stringify(budgets),
                        removedJson: JSON.stringify(removed || [])
                    }
                }).then(function (response) {
                    return response.json();
                }).then(function (result) {
                    if (self.summary) {
                        self.summary.CategoryBudgets = result.budgets || budgets;
                        self.summary.RemovedBudgetCategories = result.removed || removed || [];
                    }
                    if (!silent) {
                        Alkami.FlashBanner.showSuccess(text('budgetsSaved'));
                    }
                }).catch(function (error) {
                    showAjaxError(error);
                });
            },
            dismissInsight: function () {
                var self = this;
                if (!this.summary || !this.summary.InsightId) {
                    return Promise.resolve();
                }
                return Alkami.Helpers.ajax({
                    url: DISMISS_URL,
                    method: 'POST',
                    contentType: 'application/x-www-form-urlencoded',
                    responseType: 'json',
                    data: { insightId: this.summary.InsightId }
                }).then(function () {
                    self.summary.Insight = null;
                    self.summary.InsightId = null;
                }).catch(function (error) {
                    showAjaxError(error);
                });
            }
        }
    });

    var InsightCard = {
        name: 'InsightCard',
        template:
            '<div class="iris-card yourmonth-insight" v-if="summary && summary.Insight">' +
            '  <p>{{ summary.Insight }}</p>' +
            '  <button type="button" class="iris-button iris-button--ghost" data-modifier="compressed" @click="dismissInsight">' +
            '    <span class="iris-button__text">{{ labels.dismiss }}</span>' +
            '  </button>' +
            '</div>',
        computed: Object.assign(
            { labels: function () { return window.team3YourMonthText || {}; } },
            Pinia.mapState(useYourMonthStore, ['summary'])
        ),
        methods: Pinia.mapActions(useYourMonthStore, ['dismissInsight'])
    };

    // Draws a currency total above each bar; Chart.js core has no data labels.
    var barTotalsPlugin = {
        id: 'yourmonthTotals',
        afterDatasetsDraw: function (chart) {
            var totals = (chart.options.plugins.yourmonthTotals || {}).totals || [];
            var ctx = chart.ctx;
            totals.forEach(function (total, index) {
                if (total === null || total === undefined) { return; }
                var x = null;
                for (var d = 0; d < chart.data.datasets.length; d++) {
                    var element = chart.getDatasetMeta(d).data[index];
                    if (element && chart.data.datasets[d].data[index] !== null) { x = element.x; break; }
                }
                if (x === null) { return; }
                ctx.save();
                ctx.textAlign = 'center';
                ctx.textBaseline = 'bottom';
                ctx.fillStyle = '#333f4d';
                ctx.font = '600 12px "Segoe UI", sans-serif';
                ctx.fillText(money(total), x, chart.scales.y.getPixelForValue(total) - 6);
                ctx.restore();
            });
        }
    };

    var SpendOverview = {
        name: 'SpendOverview',
        template:
            '<div class="iris-card pad--base mar-bottom--base">' +
            '  <h3 class="font-small-heading mar-bottom--small">{{ labels.spendHeading }}</h3>' +
            '  <div class="yourmonth-spend__stats">' +
            '    <div class="yourmonth-spend__stat yourmonth-spend__stat--months">' +
            '      <div class="font-small-text">{{ labels.change }}</div>' +
            '      <div class="font-medium-heading">{{ percentChangeDisplay }}</div>' +
            '    </div>' +
            '    <div class="yourmonth-spend__stat yourmonth-spend__stat--budget" v-if="hasBudgets">' +
            '      <div class="font-small-text">{{ labels.budgetRemaining }}</div>' +
            '      <div class="font-medium-heading" :class="{ \'yourmonth-spend__negative\': remainingBudget < 0 }">{{ money(remainingBudget) }}</div>' +
            '    </div>' +
            '  </div>' +
            '  <div class="mar-top--base">' +
            '    <canvas ref="chart" height="120" role="img" :aria-label="labels.chartLabel"></canvas>' +
            '  </div>' +
            '</div>',
        computed: Object.assign(
            {
                labels: function () { return window.team3YourMonthText || {}; },
                budgetsKey: function () {
                    return JSON.stringify((this.summary && this.summary.CategoryBudgets) || {});
                },
                hasBudgets: function () {
                    return Object.keys(this.summary.CategoryBudgets || {}).length > 0;
                },
                totalBudget: function () {
                    var budgets = this.summary.CategoryBudgets || {};
                    return Object.keys(budgets).reduce(function (sum, name) { return sum + budgets[name]; }, 0);
                },
                remainingBudget: function () {
                    // Total budget compared against this month's total spend.
                    return this.totalBudget - (this.summary.ThisMonthSpend || 0);
                }
            },
            Pinia.mapState(useYourMonthStore, ['summary', 'percentChangeDisplay'])
        ),
        beforeDestroy: function () {
            if (this.chart) {
                this.chart.destroy();
            }
        },
        mounted: function () {
            this.renderChart();
        },
        watch: {
            budgetsKey: function () {
                this.renderChart();
            }
        },
        methods: {
            money: money,
            renderChart: function () {
                if (typeof Chart === 'undefined' || !this.$refs.chart) {
                    return;
                }
                if (this.chart) {
                    this.chart.destroy();
                    this.chart = null;
                }

                var chartLabels = [this.labels.lastMonth, this.labels.thisMonth];
                var spendData = [this.summary.LastMonthSpend, this.summary.ThisMonthSpend];
                // Everything shares one stack: the null slots keep the month bars
                // and the budget segments from ever stacking onto each other, and a
                // single stack group keeps each bar centered in its column.
                var datasets = [{
                    label: this.labels.spendHeading,
                    data: spendData,
                    backgroundColor: ['#9bb0c9', '#3c6e9f'],
                    stack: 'total'
                }];

                // Third bar: the configured budgets, a segment per category.
                // Category names only appear in the hover tooltip.
                var budgets = this.summary.CategoryBudgets || {};
                var budgetNames = Object.keys(budgets).sort(function (a, b) {
                    return budgets[b] - budgets[a] || a.localeCompare(b);
                });
                if (budgetNames.length) {
                    chartLabels.push(this.labels.budgetBar);
                    spendData.push(null);
                    budgetNames.forEach(function (name, index) {
                        datasets.push({
                            label: name,
                            data: [null, null, budgets[name]],
                            backgroundColor: CATEGORY_COLORS[index % CATEGORY_COLORS.length],
                            stack: 'total'
                        });
                    });
                }

                var totals = [this.summary.LastMonthSpend, this.summary.ThisMonthSpend];
                if (budgetNames.length) {
                    totals.push(this.totalBudget);
                }
                var tallest = Math.max.apply(null, totals.map(function (t) { return t || 0; }));

                this.chart = new Chart(this.$refs.chart, {
                type: 'bar',
                data: {
                    labels: chartLabels,
                    datasets: datasets
                },
                plugins: [barTotalsPlugin],
                options: {
                    plugins: {
                        legend: { display: false },
                        yourmonthTotals: { totals: totals },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.dataset.label + ': ' + money(context.parsed.y);
                                }
                            }
                        }
                    },
                    scales: {
                        x: { stacked: true },
                        y: {
                            stacked: true,
                            beginAtZero: true,
                            suggestedMax: tallest * 1.15,
                            ticks: {
                                callback: function (value) { return money(value); }
                            }
                        }
                    }
                }
                });
            }
        }
    };

    var CATEGORY_COLORS = [
        '#3c6e9f', '#e0a458', '#7fb069', '#c05850', '#8e7cc3',
        '#5ba4a4', '#d98cb3', '#9bb0c9', '#6b4f9e', '#a9a9a9'
    ];
    var FALLBACK_COLOR = '#c3c9d1';

    // One color per category, assigned in month-category order so the same
    // category gets the same color everywhere (donuts, legends, merchant list).
    function buildCategoryColors(summary) {
        var colors = {};
        var next = 0;
        ((summary && summary.LastMonthCategories) || []).concat((summary && summary.ThisMonthCategories) || []).forEach(function (c) {
            if (!(c.Name in colors)) {
                colors[c.Name] = CATEGORY_COLORS[next % CATEGORY_COLORS.length];
                next++;
            }
        });
        return colors;
    }

    // Draws the total of the currently visible slices in the donut hole; legend
    // clicks toggle slice visibility and trigger a redraw, so the total tracks them.
    var donutCenterPlugin = {
        id: 'yourmonthDonutCenter',
        afterDatasetsDraw: function (chart) {
            var arc = chart.getDatasetMeta(0).data[0];
            if (!arc) { return; }
            var total = 0;
            chart.data.datasets[0].data.forEach(function (value, index) {
                if (chart.getDataVisibility(index)) { total += value; }
            });
            var monthLabel = (chart.options.plugins.yourmonthDonutCenter || {}).label || '';
            var ctx = chart.ctx;
            ctx.save();
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = '#333f4d';
            ctx.font = '600 15px "Segoe UI", sans-serif';
            ctx.fillText(money(total), arc.x, arc.y - (monthLabel ? 10 : 0));
            if (monthLabel) {
                ctx.fillStyle = '#5b6470';
                ctx.font = '13px "Segoe UI", sans-serif';
                ctx.fillText(monthLabel, arc.x, arc.y + 12);
            }
            ctx.restore();
        }
    };

    var CategoryBreakdown = {
        name: 'CategoryBreakdown',
        data: function () {
            return { hidden: { last: {}, current: {} } };
        },
        template:
            '<div class="iris-card pad--base mar-bottom--base">' +
            '  <h3 class="font-small-heading mar-bottom--small">{{ labels.categoryHeading }}</h3>' +
            '  <div class="flex flex-justify--around">' +
            '    <div class="text-align--center width--50 mar-right--small">' +
            '      <div class="yourmonth-donut" v-if="summary.LastMonthCategories.length">' +
            '        <canvas ref="lastChart" role="img" :aria-label="labels.categoryHeading + \': \' + labels.lastMonth"></canvas>' +
            '      </div>' +
            '      <p class="font-small-text" v-else>{{ labels.noData }} ({{ labels.lastMonth }})</p>' +
            '      <ul class="yourmonth-donut__legend" v-if="summary.LastMonthCategories.length">' +
            '        <li v-for="(category, index) in summary.LastMonthCategories" :key="category.Name"' +
            '            :class="{ \'is-hidden\': hidden.last[category.Name] }" role="button" tabindex="0"' +
            '            :aria-pressed="(!!hidden.last[category.Name]).toString()"' +
            '            @click="toggle(\'last\', category.Name, index)" @keydown.enter.prevent="toggle(\'last\', category.Name, index)">' +
            '          <span class="yourmonth-donut__swatch" :style="{ backgroundColor: colorByCategory[category.Name] }" aria-hidden="true"></span>' +
            '          {{ category.Name }}' +
            '        </li>' +
            '      </ul>' +
            '    </div>' +
            '    <div class="text-align--center width--50">' +
            '      <div class="yourmonth-donut" v-if="summary.ThisMonthCategories.length">' +
            '        <canvas ref="thisChart" role="img" :aria-label="labels.categoryHeading + \': \' + labels.thisMonth"></canvas>' +
            '      </div>' +
            '      <p class="font-small-text" v-else>{{ labels.noData }} ({{ labels.thisMonth }})</p>' +
            '      <ul class="yourmonth-donut__legend" v-if="summary.ThisMonthCategories.length">' +
            '        <li v-for="(category, index) in summary.ThisMonthCategories" :key="category.Name"' +
            '            :class="{ \'is-hidden\': hidden.current[category.Name] }" role="button" tabindex="0"' +
            '            :aria-pressed="(!!hidden.current[category.Name]).toString()"' +
            '            @click="toggle(\'current\', category.Name, index)" @keydown.enter.prevent="toggle(\'current\', category.Name, index)">' +
            '          <span class="yourmonth-donut__swatch" :style="{ backgroundColor: colorByCategory[category.Name] }" aria-hidden="true"></span>' +
            '          {{ category.Name }}' +
            '        </li>' +
            '      </ul>' +
            '    </div>' +
            '  </div>' +
            '</div>',
        computed: Object.assign(
            {
                labels: function () { return window.team3YourMonthText || {}; },
                colorByCategory: function () {
                    return buildCategoryColors(this.summary);
                }
            },
            Pinia.mapState(useYourMonthStore, ['summary'])
        ),
        beforeDestroy: function () {
            var refs = this.chartRefs || {};
            Object.keys(refs).forEach(function (key) { if (refs[key]) { refs[key].destroy(); } });
        },
        mounted: function () {
            if (typeof Chart === 'undefined') {
                return;
            }
            this.chartRefs = {
                last: this.renderDonut(this.$refs.lastChart, this.summary.LastMonthCategories, this.labels.lastMonth),
                current: this.renderDonut(this.$refs.thisChart, this.summary.ThisMonthCategories, this.labels.thisMonth)
            };
        },
        methods: {
            toggle: function (side, name, index) {
                var chart = (this.chartRefs || {})[side];
                if (!chart) { return; }
                chart.toggleDataVisibility(index);
                chart.update();
                var sideMap = Object.assign({}, this.hidden[side]);
                sideMap[name] = !sideMap[name];
                this.hidden = Object.assign({}, this.hidden);
                this.hidden[side] = sideMap;
            },
            renderDonut: function (canvas, categories, monthLabel) {
                if (!canvas || !categories.length) {
                    return null;
                }
                var colorByCategory = this.colorByCategory;
                var budgets = this.summary.CategoryBudgets || {};
                return new Chart(canvas, {
                    type: 'doughnut',
                    plugins: [donutCenterPlugin],
                    data: {
                        labels: categories.map(function (c) { return c.Name; }),
                        datasets: [{
                            data: categories.map(function (c) { return c.Total; }),
                            backgroundColor: categories.map(function (c) { return colorByCategory[c.Name]; })
                        }]
                    },
                    options: {
                        // The canvas holds only the donut (legend is our own HTML
                        // below it), so both charts fill identical containers and
                        // stay the same size and vertically aligned.
                        maintainAspectRatio: false,
                        radius: '95%',
                        cutout: '68%',
                        plugins: {
                            yourmonthDonutCenter: { label: monthLabel },
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (context) {
                                        // With a budget: "Music: $11.99 of $100.00"
                                        var label = context.label + ': ' + money(context.parsed);
                                        if (context.label in budgets) {
                                            label += ' ' + (text('budgetOf') || 'of') + ' ' + money(budgets[context.label]);
                                        }
                                        return label;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }
    };

    var AmountList = {
        name: 'AmountList',
        props: {
            heading: { type: String, required: true },
            items: { type: Array, required: true },
            suffix: { type: String, default: '' },
            colorMap: { type: Object, default: null },
            icon: { type: String, default: 'swatch' },
            footerLabel: { type: String, default: '' }
        },
        template:
            '<div class="iris-card pad--base mar-bottom--base width--50" :class="cardClass">' +
            '  <h3 class="font-small-heading mar-bottom--small">{{ heading }}</h3>' +
            '  <ul class="list--plain" v-if="items.length">' +
            '    <li class="flex flex-justify--between pad-bottom--small" v-for="item in items" :key="item.Name">' +
            '      <span class="yourmonth-list__item">' +
            '        <span class="yourmonth-donut__swatch" v-if="colorMap && icon === \'swatch\'" :style="{ backgroundColor: colorMap[item.Category] || fallbackColor }" aria-hidden="true"></span>' +
            '        <span class="font-icon-recurring yourmonth-list__icon" v-else-if="colorMap" :style="{ color: colorMap[item.Category] || fallbackColor }" aria-hidden="true"></span>' +
            '        <span>' +
            '          <span class="yourmonth-list__name">{{ item.Name }}</span>' +
            '          <span class="yourmonth-list__category" v-if="colorMap && item.Category">{{ item.Category }}</span>' +
            '        </span>' +
            '      </span>' +
            '      <span>{{ money(item.Total !== undefined ? item.Total : item.Amount) }}{{ suffix }}</span>' +
            '    </li>' +
            '  </ul>' +
            '  <p class="font-small-text" v-else>{{ labels.noData }}</p>' +
            '  <div class="yourmonth-list__footer" v-if="footerLabel && items.length">' +
            '    <span>* {{ footerLabel }}</span>' +
            '  </div>' +
            '</div>',
        computed: {
            labels: function () { return window.team3YourMonthText || {}; },
            cardClass: function () { return this.suffix ? '' : 'mar-right--small'; },
            fallbackColor: function () { return FALLBACK_COLOR; }
        },
        methods: {
            money: money
        }
    };

    var App = {
        name: 'YourMonthApp',
        components: {
            'insight-card': InsightCard,
            'spend-overview': SpendOverview,
            'category-breakdown': CategoryBreakdown,
            'amount-list': AmountList
        },
        data: function () {
            return { activeTab: 'spending' };
        },
        template:
            '<div>' +
            '  <div class="pad--base" v-if="loading">' +
            '    <span>{{ labels.loading }}</span>' +
            '  </div>' +
            '  <div v-if="summary">' +
            '    <insight-card></insight-card>' +
            '    <nav class="iris-tabs iris-tabs--sub mar-bottom--base" :aria-label="labels.tabsLabel">' +
            '      <div class="iris-tabs__inner">' +
            '        <ul class="iris-tabs__list" role="tablist">' +
            '          <li v-for="tab in tabs" :key="tab.id" class="iris-tabs__list-item" role="tab"' +
            '              :id="\'yourmonth_tab_\' + tab.id" :aria-controls="\'yourmonth_panel_\' + tab.id"' +
            '              :aria-selected="(activeTab === tab.id).toString()" tabindex="0"' +
            '              @click="activeTab = tab.id" @keydown.enter.prevent="activeTab = tab.id" @keydown.space.prevent="activeTab = tab.id">' +
            '            <p class="iris-tabs__link">{{ tab.label }}</p>' +
            '          </li>' +
            '        </ul>' +
            '      </div>' +
            '    </nav>' +
            '    <div :id="\'yourmonth_panel_\' + activeTab" role="tabpanel" :aria-labelledby="\'yourmonth_tab_\' + activeTab">' +
            '      <spend-overview v-if="activeTab === \'spending\'"></spend-overview>' +
            '      <category-breakdown v-if="activeTab === \'categories\'"></category-breakdown>' +
            '      <div class="flex" v-if="activeTab === \'insights\'">' +
            '        <amount-list :heading="labels.topMerchants" :items="summary.TopMerchants"' +
            '                     :color-map="categoryColors" :footer-label="totalOverLabel"></amount-list>' +
            '        <amount-list :heading="labels.recurringCharges" :items="summary.RecurringCharges" suffix="/mo"' +
            '                     :color-map="categoryColors" icon="recurring"></amount-list>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>',
        computed: Object.assign(
            {
                labels: function () { return window.team3YourMonthText || {}; },
                tabs: function () {
                    return [
                        { id: 'spending', label: this.labels.spendHeading },
                        { id: 'categories', label: this.labels.categoryHeading },
                        { id: 'insights', label: this.labels.insightsTab }
                    ];
                },
                categoryColors: function () {
                    return buildCategoryColors(this.summary);
                },
                totalOverLabel: function () {
                    return (this.labels.totalOverDays || '').replace('{days}', this.summary ? this.summary.LookbackDays : '');
                }
            },
            Pinia.mapState(useYourMonthStore, ['summary', 'loading'])
        ),
        created: function () {
            useYourMonthStore().loadSummary();
        }
    };

    var SAVE_EVENT = 'team3yourmonth:savebudgets';
    var DIVIDER = '──────────';

    var BudgetForm = {
        name: 'BudgetForm',
        data: function () {
            return { edits: {}, removed: [], added: [], selectedAdd: '', saving: false };
        },
        template:
            '<div class="yourmonth-budget" v-if="summary">' +
            '  <div v-for="row in rows" :key="row.name" class="yourmonth-budget__row">' +
            '    <div class="yourmonth-budget__info">' +
            '      <div class="yourmonth-budget__name">{{ row.name }}</div>' +
            '      <div class="yourmonth-budget__spent">{{ labels.budgetSpent }}: {{ money(row.spent) }}</div>' +
            '    </div>' +
            '    <div class="yourmonth-budget__amount">' +
            '      <span class="yourmonth-budget__currency" aria-hidden="true">$</span>' +
            '      <input type="number" min="0" step="0.01" inputmode="decimal"' +
            '             v-model="edits[row.name]" @input="clampDecimals(row.name)" @blur="formatAmount(row.name)"' +
            '             :aria-label="labels.budgetAmount + \' \' + row.name" placeholder="0.00">' +
            '    </div>' +
            '    <button type="button" class="iris-button iris-button--ghost yourmonth-budget__remove" data-modifier="compressed"' +
            '            :aria-label="labels.budgetRemove + \' \' + row.name" :disabled="saving" @click="removeCategory(row.name)">' +
            '      <span class="font-icon-cancel-x" aria-hidden="true"></span>' +
            '    </button>' +
            '  </div>' +
            '  <p class="font-small-text" v-if="!rows.length">{{ labels.noData }}</p>' +
            '  <div class="yourmonth-budget__restore" v-if="addOptions.length">' +
            '    <select v-model="selectedAdd" :aria-label="labels.budgetAddCategory">' +
            '      <option value="" disabled>{{ labels.budgetAddCategory }}</option>' +
            '      <option v-for="option in addOptions" :key="option.key" :value="option.name" :disabled="option.divider">{{ option.divider ? dividerText : option.name }}</option>' +
            '    </select>' +
            '    <button type="button" class="iris-button iris-button--secondary" :disabled="!selectedAdd || saving" @click="addCategory">' +
            '      <span class="iris-button__text">{{ labels.budgetAdd }}</span>' +
            '    </button>' +
            '  </div>' +
            '</div>',
        computed: Object.assign(
            {
                labels: function () { return window.team3YourMonthText || {}; },
                dividerText: function () { return DIVIDER; },
                windowSpend: function () {
                    var spent = {};
                    var source = (this.summary.AllCategories && this.summary.AllCategories.length)
                        ? this.summary.AllCategories
                        : (this.summary.ThisMonthCategories || []);
                    source.forEach(function (c) { spent[c.Name] = c.Total; });
                    return spent;
                },
                rows: function () {
                    // Categories with spend in the window, budgeted categories, and
                    // ones the user explicitly added - minus the removed ones.
                    var spent = Object.assign({}, this.windowSpend);
                    Object.keys(this.summary.CategoryBudgets || {}).forEach(function (name) {
                        if (!(name in spent)) { spent[name] = 0; }
                    });
                    this.added.forEach(function (name) {
                        if (!(name in spent)) { spent[name] = 0; }
                    });

                    var removed = this.removed;
                    return Object.keys(spent)
                        .filter(function (name) { return removed.indexOf(name) === -1; })
                        .map(function (name) { return { name: name, spent: spent[name] }; })
                        .sort(function (a, b) { return b.spent - a.spent || a.name.localeCompare(b.name); });
                },
                addOptions: function () {
                    // Removed categories the user has spend in come first, then a
                    // divider, then the rest of the expense category tree.
                    var inRows = {};
                    this.rows.forEach(function (row) { inRows[row.name] = true; });
                    var windowSpend = this.windowSpend;

                    var removedWithSpend = this.removed
                        .filter(function (name) { return (windowSpend[name] || 0) > 0 || name in windowSpend; })
                        .sort(function (a, b) { return (windowSpend[b] || 0) - (windowSpend[a] || 0) || a.localeCompare(b); });

                    var others = (this.summary.ExpenseCategories || [])
                        .concat(this.removed)
                        .filter(function (name, index, list) {
                            return list.indexOf(name) === index
                                && !inRows[name]
                                && removedWithSpend.indexOf(name) === -1;
                        })
                        .sort(function (a, b) { return a.localeCompare(b); });

                    var options = removedWithSpend.map(function (name) { return { key: 'r:' + name, name: name }; });
                    if (removedWithSpend.length && others.length) {
                        options.push({ key: 'divider', name: '', divider: true });
                    }
                    return options.concat(others.map(function (name) { return { key: 'o:' + name, name: name }; }));
                }
            },
            Pinia.mapState(useYourMonthStore, ['summary'])
        ),
        watch: {
            summary: {
                immediate: true,
                handler: function (value) {
                    if (!value) { return; }
                    this.removed = (value.RemovedBudgetCategories || []).slice();
                    var budgets = value.CategoryBudgets || {};
                    var edits = {};
                    this.rows.forEach(function (row) {
                        edits[row.name] = (row.name in budgets) ? Number(budgets[row.name]).toFixed(2) : '';
                    });
                    this.edits = edits;
                }
            },
            saving: function (value) {
                var saveButton = document.getElementById('yourmonth_budget_save');
                if (saveButton) {
                    saveButton.disabled = value;
                }
            }
        },
        methods: {
            money: money,
            clampDecimals: function (name) {
                var value = this.edits[name] || '';
                var dot = value.indexOf('.');
                if (dot > -1 && value.length - dot - 1 > 2) {
                    this.edits[name] = value.substring(0, dot + 3);
                }
            },
            formatAmount: function (name) {
                var value = parseFloat(this.edits[name]);
                this.edits[name] = (!isNaN(value) && value > 0) ? value.toFixed(2) : '';
            },
            collectBudgets: function () {
                var budgets = {};
                var removed = this.removed;
                Object.keys(this.edits).forEach(function (name) {
                    var value = parseFloat(this.edits[name]);
                    if (removed.indexOf(name) === -1 && !isNaN(value) && value > 0) {
                        budgets[name] = Math.round(value * 100) / 100;
                    }
                }, this);
                return budgets;
            },
            persist: function (silent) {
                var self = this;
                this.saving = true;
                return useYourMonthStore().saveBudgets(this.collectBudgets(), this.removed, silent).then(function () {
                    self.saving = false;
                });
            },
            save: function () {
                this.persist(false);
            },
            removeCategory: function (name) {
                if (this.removed.indexOf(name) === -1) {
                    this.removed.push(name);
                }
                this.added = this.added.filter(function (n) { return n !== name; });
                // Removing a category zeroes out its budget.
                var edits = Object.assign({}, this.edits);
                edits[name] = '';
                this.edits = edits;
                this.persist(true);
            },
            addCategory: function () {
                var name = this.selectedAdd;
                if (!name) { return; }
                this.removed = this.removed.filter(function (n) { return n !== name; });
                if (this.added.indexOf(name) === -1) {
                    this.added.push(name);
                }
                var edits = Object.assign({}, this.edits);
                if (!(name in edits)) { edits[name] = ''; }
                this.edits = edits;
                this.selectedAdd = '';
                this.persist(true);
            }
        },
        created: function () {
            document.addEventListener(SAVE_EVENT, this.save);
        },
        beforeDestroy: function () {
            document.removeEventListener(SAVE_EVENT, this.save);
        }
    };

    function wireBudgetDrawer() {
        var drawerElement = document.getElementById('yourmonth_budget_drawer');
        var gearButton = document.getElementById('yourmonth_settings_button');
        if (!drawerElement || !gearButton || !Alkami.Iris || !Alkami.Iris.DrawerComponent) {
            return;
        }

        Alkami.Iris.DrawerComponent.init(drawerElement);
        gearButton.addEventListener('click', function () {
            Alkami.Iris.DrawerComponent.componentForElement(drawerElement).open = true;
        });

        var saveButton = document.getElementById('yourmonth_budget_save');
        if (saveButton) {
            saveButton.addEventListener('click', function () {
                document.dispatchEvent(new CustomEvent(SAVE_EVENT));
            });
        }
    }

    Alkami.Dom.onDocumentReady(function () {
        var mountElement = document.getElementById('app');
        if (!mountElement || typeof Vue === 'undefined' || typeof Pinia === 'undefined') {
            return;
        }

        wireBudgetDrawer();

        Vue.use(Pinia.PiniaVuePlugin);
        var pinia = Pinia.createPinia();
        new Vue({
            el: mountElement,
            pinia: pinia,
            render: function (createElement) { return createElement(App); }
        });

        var budgetElement = document.getElementById('yourmonth_budget_app');
        if (budgetElement) {
            new Vue({
                el: budgetElement,
                pinia: pinia,
                render: function (createElement) { return createElement(BudgetForm); }
            });
        }
    });
})();
