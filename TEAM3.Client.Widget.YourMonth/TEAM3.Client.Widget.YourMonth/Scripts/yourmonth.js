(function () {
    'use strict';

    var summaryUrl = '/TEAM3YourMonth/Summary';
    var dismissUrl = '/TEAM3YourMonth/DismissInsight';

    function el(id) {
        return document.getElementById(id);
    }

    function money(value) {
        return Alkami.Utils.CurrencyHelper.formatCurrency(value || 0);
    }

    function renderList(listId, emptyId, items, renderItem) {
        var list = el(listId);
        var empty = el(emptyId);
        list.innerHTML = '';
        if (!items || items.length === 0) {
            empty.hidden = false;
            return;
        }
        empty.hidden = true;
        items.forEach(function (item) {
            var li = document.createElement('li');
            li.className = 'flex flex-justify--between pad-bottom--small';
            renderItem(li, item);
            list.appendChild(li);
        });
    }

    function renderChart(summary) {
        if (typeof Chart === 'undefined') {
            return;
        }
        var canvas = el('yourmonth_chart');
        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: ['Last month', 'This month'],
                datasets: [{
                    label: 'Spending',
                    data: [summary.LastMonthSpend, summary.ThisMonthSpend],
                    backgroundColor: ['#9bb0c9', '#3c6e9f']
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) { return money(value); }
                        }
                    }
                }
            }
        });
    }

    function renderInsight(summary) {
        var card = el('yourmonth_insight');
        if (!summary.Insight) {
            card.hidden = true;
            return;
        }
        el('yourmonth_insight_text').textContent = summary.Insight;
        card.hidden = false;

        el('yourmonth_insight_dismiss').addEventListener('click', function () {
            Alkami.Helpers.ajax({
                url: dismissUrl,
                method: 'POST',
                contentType: 'application/x-www-form-urlencoded',
                responseType: 'json',
                data: { insightId: summary.InsightId }
            }).then(function () {
                card.hidden = true;
            }).catch(function (error) {
                showAjaxError(error);
            });
        });
    }

    function render(summary) {
        el('yourmonth_this_month').textContent = money(summary.ThisMonthSpend);
        el('yourmonth_last_month').textContent = money(summary.LastMonthSpend);
        el('yourmonth_percent_change').textContent = summary.PercentChange === null
            ? '—'
            : (summary.PercentChange > 0 ? '+' : '') + summary.PercentChange + '%';

        renderList('yourmonth_merchants', 'yourmonth_merchants_empty', summary.TopMerchants, function (li, m) {
            var name = document.createElement('span');
            name.textContent = m.Name;
            var total = document.createElement('span');
            total.textContent = money(m.Total);
            li.appendChild(name);
            li.appendChild(total);
        });

        renderList('yourmonth_recurring', 'yourmonth_recurring_empty', summary.RecurringCharges, function (li, r) {
            var name = document.createElement('span');
            name.textContent = r.Name;
            var amount = document.createElement('span');
            amount.textContent = money(r.Amount) + '/mo';
            li.appendChild(name);
            li.appendChild(amount);
        });

        renderInsight(summary);
        renderChart(summary);

        el('yourmonth_loading').hidden = true;
        el('yourmonth_content').hidden = false;
    }

    function showAjaxError(error) {
        if (error instanceof Response) {
            error.json().then(function (data) {
                Alkami.FlashBanner.showError(data.errorMessage);
            }).catch(function () {
                Alkami.FlashBanner.showError('Something went wrong. Please try again later.');
            });
            return;
        }
        Alkami.FlashBanner.showError('Something went wrong. Please try again later.');
    }

    function loadSummary() {
        Alkami.Helpers.ajax({
            url: summaryUrl,
            responseType: 'json'
        }).then(function (response) {
            return response.json();
        }).then(function (summary) {
            render(summary);
        }).catch(function (error) {
            el('yourmonth_loading').hidden = true;
            showAjaxError(error);
        });
    }

    Alkami.Dom.onDocumentReady(function () {
        if (el('yourmonth_content')) {
            loadSummary();
        }
    });
})();
