# Front End API, Iris Design System, and Vue in Widgets

**What this covers.** The client-side building blocks available to an Alkami SDK widget: the Iris 2 / Iris Vue design system and how to load Vue.js and Iris assets in a widget view; the global `Alkami.*` JavaScript namespaces (`Alkami.Dom`, `Alkami.Helpers`, `Alkami.Localization`, `Alkami.Patterns`, `Alkami.Security`, `Alkami.Utils`); the global components (`Alkami.FlashBanner`, `Alkami.WidgetHeader`, the conceal-elements co-browsing abstraction, the mobile titlebar and `window.nativeHook` adapter); the supported chart library (Chart.js and vue-chartjs); and the supported ECMAScript level, polyfills, and features to avoid. The Front End API pages cover the "helpers, non-Iris abstractions, and adapters used in the Alkami Platform front end"; source lives in the `orb-shell-js` repository (https://bitbucket.corp.alkamitech.com/projects/UP/repos/orb-shell-js/browse). All Front End API pages were last updated 2023-10-11.

## 1. Iris Design System (Iris 2 / Iris Vue)

- The current component libraries are **Iris 2**. Design principles and per-component design docs (over 35 components): https://www.alkami.design/ and https://www.alkami.design/components.
- **Iris Vue** is the Vue.js 2.x implementation of Iris 2, available to SDK clients since February 2022. It is the same library Alkami uses internally, so custom widgets match Alkami-built experiences. Technical component docs: https://iris-vue.alkami.design.
- The original **Iris Classic** library (https://iris.alkamitech.com/) is end of life, replaced by Iris 2 / Iris Vue. Several Front End API helpers still return Iris Classic style objects (for example `createDialog` returns an `Iris.PromptComponent` and uses `iris-prompt` markup); those helpers remain in the platform.

Sources: Iris Design System, https://confluence.alkami.com/spaces/SDKC/pages/55349278

## 2. Using Vue.js in an SDK Widget

Alkami uses Vue.js for the client-side code of most widgets. Vue is not required, but using it gives access to the Iris.Vue (Iris 2) components. Rules (page updated 2024-04-10):

- Before mid-2022 Vue had to be loaded from a public CDN; now the files are served from Alkami servers. Alkami strongly encourages using the Alkami-provided files.
- A **dashboard module must not** include Vue (or any other client framework) from a public CDN if Alkami already loads it on the page; doing so causes version conflicts.
- Use the `Alkami.Client.WebClient.Shared.Helpers` HTML helpers so all files get cache-expiration handling: `@Html.IrisVueLinksSnippet()` (Iris Vue CSS, in `StyleSheetContentPlaceholder`), `@Html.IrisVueScriptsSnippet()` (Iris Vue JS, after `vue.min.js` in `JavaScriptIncludeContentPlaceholder`), `@Html.ScriptWithCacheExpiration(path)` and `@Html.CssWithCacheExpiration(path)` for your own files or CDN files.

```
@using Alkami.Client.WebClient.Shared.Helpers

@section TitleContentPlaceholder{
@Html.SiteText("Title")
}

@section StyleSheetContentPlaceholder{
@Html.IrisVueLinksSnippet()
@Html.CssWithCacheExpiration("~/Areas/SampleAccounts/Styles/SampleAccounts.css")
}

@section JavaScriptIncludeContentPlaceholder{
@Html.ScriptWithCacheExpiration("~/lib/vue/vue.min.js")
@Html.IrisVueScriptsSnippet()
@Html.ScriptWithCacheExpiration("~/Areas/SampleMyWidget/Scripts/SampleSpecialScripting.js")
<script>
new Vue({ ... // continue on with your Vue.js code here
```

Sources: Using Vue.js in an SDK Widget, https://confluence.alkami.com/spaces/SDKC/pages/232198017

## 3. Alkami.Dom

DOM helpers (hide/show, fades, slides, event emission). Animated methods default to 400 ms and accept an optional `finishFunction` callback.

| Method | Returns | Notes |
|---|---|---|
| `ancestors(element, [query])` | `HTMLElement[]` | All ancestors, optionally filtered by selector. For one ancestor use `element.closest`. |
| `dispatchEvent(target, eventName, [detail = {}], [bubbles = true], [cancelable = true])` | `void` | Emits a `CustomEvent`. |
| `fadeIn(element, [duration=400], finishFunction?)` / `fadeOut(...)` / `fadeToggle(...)` | `void` | Via `opacity`. |
| `slideDown(element, [duration=400], finishFunction?)` / `slideUp(...)` / `slideToggle(...)` | `void` | Via height. |
| `hideElement(element)` / `showElement(element)` | `void` | Immediate `display: none` / `display: block`. |
| `isVisible(element)` | `boolean \| null` | |
| `onDocumentReady(callbackFunction)` | `void` | Same semantics as jQuery document ready. |
| `parseHTML(htmlString)` | `HTMLElement` | **Only one root node allowed.** |
| `passiveEventListener(query, listenerFn: (event, passiveTarget) => void)` | `EventListener` | Delegated listener like jQuery `.on(selector)`; pass the result to `addEventListener` / `removeEventListener`. Use for elements not yet in the DOM. |
| `removeChildNodes(parentElement)` | `void` | Clears all children. |
| `sanitizeHTML(html: string)` | `string` | Strips harmful HTML (`<a href="javascript:...">` becomes `<a>`). |
| `sanitizeHTMLRecursively(object)` | sanitized copy | Sanitizes every string property and array element. |

```
Alkami.Dom.onDocumentReady(() => { Alkami.Dom.hideElement(document.getElementById('promo')); });

const elementToFade = document.getElementById('fade_element');
Alkami.Dom.fadeIn(elementToFade, 200, () => { elementToFade.setAttribute('aria-expanded', 'true'); });

// Delegated listener; the second parameter (or `this`) is the matched target
const listener = function(event, target) {
  if (!target.href) { return; }
  event.preventDefault();
  Alkami.Helpers.postLink(target.href);
};
document.addEventListener('click', Alkami.Dom.passiveEventListener('a.post-link', listener));

document.body.appendChild(Alkami.Dom.parseHTML('<div class="iris-prompt" aria-hidden="true" data-size="small">...</div>'));
```

Sources: Alkami DOM, https://confluence.alkami.com/spaces/SDKC/pages/303531337

## 4. Alkami.Helpers

### uniqueID() and KEY_CODES

- `Alkami.Helpers.uniqueID()` returns a random GUID-style string such as `'f05a556e-7e25-f958-ad98-2fc635192468'`.
- `Alkami.Helpers.KEY_CODES` maps names to `keyCode` / `which` values: `BACKSPACE`, `COMMA`, `DELETE`, `DASH`, `DOWN`, `END`, `ENTER`, `ESCAPE`, `HOME`, `LEFT`, `MINUS`, `PAGE_DOWN`, `PAGE_UP`, `PERIOD`, `RIGHT`, `SPACE`, `TAB`, `UP`. Usage: `if (event.which === Alkami.Helpers.KEY_CODES.DELETE) {...}`. Note `keyCode` and `which` are deprecated in favor of `KeyboardEvent.key`.

### ajax(options)

Wrapper around `fetch` returning `Promise<Response>`. The promise **rejects** (with the `Response`) if the request fails or the status is not in the 200s.

```
interface AjaxOptions {
  url: string;
  contentType?: string;   // application/x-www-form-urlencoded | multipart/form-data | application/json
  headers?: { [key: string]: string };
  data?: string | {} | FormData | URLSearchParams;
  responseType?: 'xml' | 'html' | 'json' | 'text';
  method: 'GET' | 'HEAD' | 'POST' | 'PUT' | 'DELETE' | 'CONNECT' | 'OPTIONS' | 'TRACE' | 'PATCH'; // GET by default
}
```

Defaults: `{ method: 'GET' }`. If `contentType` is omitted it is inferred from `data`: `Object` gives `application/json`, `FormData` gives `multipart/form-data`, `URLSearchParams` gives `application/x-www-form-urlencoded`. Data is serialized to match (`{a: 'a', b: 1}` as urlencoded sends `a=a&b=1`); for GET, `data` becomes the query string (`/Tasks?taskId=1`).

```
// GET with query string
const response = await Alkami.Helpers.ajax({ url: '/Tasks', responseType: 'json', data: { taskId } });
const data = await response.json();

// POST as a form, handling rejection
async function createTask(data) {
  try {
    const response = await Alkami.Helpers.ajax({
      url: '/Tasks', method: 'POST', contentType: 'application/x-www-form-urlencoded', responseType: 'json', data
    });
    return await response.json();
  } catch (error) {
    if (error instanceof Response) {
      const errorData = await error.json();
      Alkami.FlashBanner.showError(errorData.errorMessage);
      return errorData;
    }
    throw error;
  }
}
```

Passing a `FormData` object as `data` (for example `formElement.formData`) sends `multipart/form-data`, which suits file fields. Promise form: `.then((response) => response.json()).catch((error) => error instanceof Response ? error.json().then(...) : Promise.reject(error))`.

### createCache(cacheKey: string)

Returns a `Cache` stored in session storage if available, otherwise in memory.

```
interface Cache {
  readonly key: string;   // Key the cache is stored in
  value: string;          // get or set value
  invalidate(): void;     // completely remove value from storage
}

const cache = Alkami.Helpers.createCache('Alkami.Globals');
cache.value = await (await fetch('/API/Globals')).text();
const globals = JSON.parse(cache.value);  // later
```

### createDialog(options)

Programmatically creates an Iris prompt. Returns `Iris.PromptComponent`; set `.open = true` to show it. The prompt element emits `iris.prompt.closed`.

```
interface IDialogOptions {
  id?: string;                     // ID attribute for the prompt
  cls?: string;                    // Class attribute for the prompt
  content: string | HTMLElement;   // HTML string or element injected into the prompt body
  destroyOnClose?: boolean;        // Remove the prompt element from the DOM when closed
  size?: string;                   // 'small', 'medium', 'large'
  title?: string;
  handlers: { [key: string]: EventListener }; // listeners referenced by button handlerName
  buttons: IDialogButtonOptions[];            // buttons created in the prompt footer
}

export interface IDialogButtonOptions {
  cls?: string;
  handlerName: string;   // key in options.handlers called when activated
  href?: string;         // if used, tag should be 'a'
  id?: string;
  tag?: string;
  text?: string;
  type?: string;         // iris-button type: 'primary' | 'secondary'
  closePrompt?: boolean; // close the prompt when clicked
}
```

Defaults: `{ id: 'alkami_modal_' + modalCount, cls: '', content: '', destroyOnClose: false, size: 'large', title: '', handlers: {}, buttons: [] }`.

```
// Simple dialog, no buttons
const dialog = Alkami.Helpers.createDialog({
  id: 'ad-blocker-detection-dialog', title: 'Ad Blocker Detected', size: 'large', destroyOnClose: true,
  content: '<p>We recommend you disable ad blocker for the best experience.</p>'
});
document.getElementById('ad-blocker-detection-dialog').addEventListener('iris.prompt.closed', event => {
  localStorage.setItem('adBlockerPromptSeen', 'true');
});
dialog.open = true;

// Buttons wired to named handlers (session timeout prompt)
const sessionPrompt = Alkami.Helpers.createDialog({
  id: 'session-dialog', title: 'Are you still there?', content: sessionBodyMarkup,
  handlers: {
    'logout': function(e) { window.location.href = "/Logout"; },
    'cancel': function(e) { idleTimeout.resetTimer(); }
  },
  buttons: [
    { text: 'Logout', type: 'secondary', handlerName: 'logout', closePrompt: false },
    { text: "I'm still here!", handlerName: 'cancel', closePrompt: true }
  ]
});
```

### Dialog presets

Each returns `Iris.PromptComponent` and presets only `buttons`; other `IDialogOptions` go in `options`. Button labels come from Site Text keys.

- `createCloseDialog(content, options)`: one "Okay" button (`Modal.Confirm.Button.Okay`, `handlerName: 'close'`, `closePrompt: true`). Pass a handler named `close` to react.
- `createYesNoDialog(content, options)`: "No" (`Modal.Confirm.Button.No`, `handlerName: 'no'`, `type: 'secondary'`) and "Yes" (`Modal.Confirm.Button.Yes`, `handlerName: 'yes'`), both `closePrompt: true`. Pass handlers named `yes` / `no`.
- `createConfirmationDialog(content, confirmationText, options)`: "Cancel" (`Modal.Confirm.Button.Cancel`, `handlerName: 'cancel'`, `type: 'secondary'`) and a confirm button (`Modal.Confirm.Button.Confirm`, `handlerName: 'delete'`), both `closePrompt: true`. The page's note says to pass handlers named `close` or `delete`, while its example passes `'confirmation'`; the defaults show the confirm button calls `'delete'`, so use that.

```
Alkami.Helpers.createCloseDialog(`<div>Body content for the prompt.</div>`, { title: 'Informational' });
Alkami.Helpers.createYesNoDialog(`<div>Are you sure you want to logout?</div>`, { title: 'Are you sure?' });
Alkami.Helpers.createConfirmationDialog(`<div>Deleting this is destructive. Are you sure?</div>`, 'Delete',
  { title: 'Are you sure?', handlers: { 'delete': function(e) { /* do something destructive */ } } });
```

### Form helpers

- `bindPostLink(element: HTMLElement)`: binds a link to use `postLink`. Example: `Array.from(document.querySelectorAll('a.post-link')).forEach((el) => bindPostLink(el));`
- `disableSubmit(element: HTMLElement, disabledText: string)`: disables a submit button and swaps its text. `enableSubmit(element)` restores it.
- `postLink(url: string | URL)`: makes a POST instead of a GET for a URL. Use for links with sensitive data or disposable forms. Example: `postLink('/AccountV2/Transactions?accountId=123&startDate=2018-01-01&endDate=2018-12-01');`

The page's examples call these unqualified; they live on `Alkami.Helpers` (for example `Alkami.Helpers.postLink`).

Sources: Alkami Helpers, https://confluence.alkami.com/spaces/SDKC/pages/303531338

## 5. Alkami.Localization

`Alkami.Localization.SiteText` exposes localized, configurable Site Text for the current locale. By default all global Site Text plus the current widget's Site Text is available.

- `SiteText.get(siteTextKey, arg1?, arg2?, ...)`: returns the string, or an **empty string** if the key is missing. Extra arguments fill 0-indexed placeholders (`{0}`). Preferred.
- `SiteText[siteTextKey]`: returns the string or **`undefined`**, no placeholder support. Widely seen in the codebase but **discouraged**.

```
userNoticeElement.innerText = Alkami.Localization.SiteText.get('Widget.User.Notice');
// Widget.Disclaimer.Text = 'Hello {0}, Please accept our disclaimer.'
disclaimerElement.innerText = Alkami.Localization.SiteText.get('Widget.Disclaimer.Text', 'Mike Brady');
element.innerText = Alkami.Localization.SiteText['Widget.User.Notice'] || '';   // discouraged
```

Sources: Alkami Localization, https://confluence.alkami.com/spaces/SDKC/pages/303531339

## 6. Alkami.Patterns

Shared regex patterns; more are to be added via PR to `orb-shell-js`. Only `Alkami.Patterns.POBox` (`RegExp`, validates a PO Box address) is documented.

```
if (address1.match(Alkami.Patterns.POBox)) {
  errors.push(Alkami.Localization.SiteText["Addresses.CannotBePOBox"]);
}
```

Sources: Alkami Patterns, https://confluence.alkami.com/spaces/SDKC/pages/303531340

## 7. Alkami.Security (MFA)

### post(options: SecurityAjaxOptions)

Performs an ajax POST and determines whether MFA step-up is required, showing the MFA modal by default. Returns `Promise<Response>`; rejects on failure or non-2xx status. **A user clicking the MFA cancel button is a rejection, so always catch.** `SecurityAjaxOptions extends AjaxOptions` with `method` defaulting to `'POST'` plus:

```
mfaAction?: function   // initializes the MFA component for you. Defaults to using initMFAModal
```

```
Alkami.Security.post({
  url: '/Settings/Security/UpdatePassword',
  data: { PasswordCurrent: 'Test12345!', PasswordNew: 'Test12345!', PasswordConfirmation: 'Test12345!' },
  contentType: 'application/x-www-form-urlencoded'
}).then((request) => request.json())
  .then((body) => console.log(body))
  .catch((error) => console.error(error));
```

### MFAComponent (using MFA outside a modal)

Supply `mfaAction` in the `post` options; it receives `{ verificationToken, transactionId }` and must return a promise that resolves when MFA succeeds.

```
mfaAction: (data) => {
  const { verificationToken, transactionId } = data;
  const containerElement = document.getElementById('element_to_insert_mfa');
  let mfaComponent;
  return Alkami.Security.MFAComponent.init({
    submitButtonElement: document.getElementById('button_to_submit_mfa_forms'),
    escalationKey: verificationToken,
    transactionId: transactionId
  }).then((mfa) => {
    mfaComponent = mfa;
    containerElement.appendChild(mfaComponent.element);
    return mfaComponent.promise;
  }).then(() => { mfaComponent.destroy(); });
},
```

API:

- `MFAComponent.init(options: MFAComponentOptions)` returns `Promise<MFAComponent>` (preferred).
- `MFAComponent.insertStyles()` returns `Promise<HTMLLinkElement>`; installs the component CSS.
- `new MFAComponent(options: MFAComponentOptions & IMFAModelResponse)` returns `MFAComponent`.
- Instance: `element` (`HTMLElement`), `promise` (`Promise<void>`, resolves on MFA success), `destroy()`.

```
interface MFAComponentOptions {
  submitButtonElement: HTMLButtonElement; // Button to tie form functions to
  escalationKey?: string;                 // Escalation Key to send with MFA answers
  transactionId?: string;                 // Transaction ID to send with MFA answers
}
```

`IMFAModelResponse` fields: `BankName: string`, `BodyClasses: string`, `CurrentUserTimeZone: number | null`, `DefaultStepUpType: number`, `HasFilters: boolean`, `HashedUserIdentifier: string | null`, `IdleLogoutMinutes: number`, `IsLastPage: boolean`, `PasswordExpirationInMinutes: string`, `RelationshipCode: string | null`, `TotalCount: number`, `TransactionId: string | null`, `TransactionType: string | null`, `Types: IAuthenticationTypeResponse[]`, `UserIdentifier: string`, `UserName: string`, `UserPortraitImageSignature: string | null`.

Sources: Alkami Security, https://confluence.alkami.com/spaces/SDKC/pages/303531341

## 8. Alkami.Utils

Low-level single-purpose helpers grouped into `CookieHelper`, `CurrencyHelper`, `DateHelper`, `StorageHelper`, `StringHelper`.

**Alkami.Utils.CookieHelper**: `createCookie(name, value, days?)`, `readCookie(name)` returns `string | null`, `eraseCookie(name)`, `eraseAllCookies()`.

```
if (!Alkami.Utils.CookieHelper.readCookie('browser_unsupported')) {
  Alkami.Utils.CookieHelper.createCookie('browser_no_warn', 1);
}
```

**Alkami.Utils.CurrencyHelper**: `parseCurrency(string)` returns `number` (`'$34,545.43'` gives `34545.43`); `formatCurrency(value: number, options?: IFormatCurrencyOptions)` returns `string`.

```
interface IFormatCurrencyOptions {
  useGroupingSeparators?: boolean;        // default true  (false => $1000000.00)
  fractionalDigitsRequired?: boolean;     // default true  (false => $1, but 1.23 stays $1.23)
  showCurrencyIndicator?: boolean;        // default true  (false => 1.23)
  useParenthesisNegativeMarker?: boolean; // default false (true => ($3.24) instead of -$3.24)
}
Alkami.Utils.CurrencyHelper.formatCurrency(1000000);   // => $1,000,000.00
```

**Alkami.Utils.DateHelper** (all `add*` return a new `Date`): `addDays(date, days)`, `addHours(date, hours)`, `addMilliseconds(date, ms)`, `addMinutes(date, minutes)`, `addMonths(date, months)` (clamps to month end: `2020-01-31` + 1 = `2020-02-29`), `addSeconds(date, seconds)`, `addWeeks(date, weeks)`, `getDaysInMonth(date)` returns `number`, `getIsoDate(date)` returns `yyyy-mm-dd` (timezone shift can move the day: page shows `new Date('2044-06-26')` giving `'2044-06-25'`), `isDate(any)` returns `boolean` (`false` for invalid dates), `isLeapYear(date)`, `monthsBetween(arg1, arg2)` returns signed `number` (positive when arg2 is later), `startOfDay(date)` and `today()` return midnight in the current timezone.

**Alkami.Utils.StorageHelper**: `hasStorage(type: 'local' | 'session')`, `hasLocalStorage()`, `hasSessionStorage()`, all `boolean`. `false` usually means incognito mode.

**Alkami.Utils.StringHelper**:

| Method | Example |
|---|---|
| `camelize(word, [firstLetterInUppercase = true])` | `camelize('camels_are_very_cool', false)` gives `"camelsAreVeryCool"`; `true` gives `"CamelsAreVeryCool"`. |
| `capitalize(text)` | `'capitalize me!'` gives `'Capitalize me!'` |
| `dasherize(word)` | `'profile_nav'` gives `'profile-nav'` |
| `format(template, ...args)` | `format('{1} and {2} are friends.', 'Spike', 'Jet')` gives `'Spike and Jet are friends.'` (1-based here, unlike `SiteText.get`). |
| `normalizePunctuation(text)` | Replaces iOS smart punctuation (curly quotes, apostrophes, em dashes) with ASCII. |
| `parameterize(word, [sep = '-'])` | `'what**a---weird\'string'` gives `"what-a-weird-string"`; `'underscores_are_okay'` unchanged. |
| `titleize(text)` | `'what***does this---do'` gives `"What Does This Do"`; underscores kept (`"What_about_underscores"`). |
| `underscore(word)` | `'UnderscorePascalCase'` gives `"underscore_pascal_case"`; `'can-this-handle-kebabcase'` gives `"can_this_handle_kebabcase"`; `'WhatAbout99MultipleCapitalLLLetters'` gives `"what_about99_multiple_capital_ll_letters"`. |

Sources: Alkami Utils, https://confluence.alkami.com/spaces/SDKC/pages/303531342

## 9. Global Components

### 9.1 Conceal Elements (co-browsing protection)

When a user shares their screen with a support agent (for example Glia co-browsing) everything on screen is shared. Widgets mark sensitive elements with **generic data attributes**; the platform adds the vendor class (Glia: `sm_cobrowsing_hidden_field`, etc.) at page load or on an event, so widget code stays vendor-agnostic.

| Attribute | Agent sees | Use for |
|---|---|---|
| `data-cobrowsing-hidden` | Nothing; element removed from agent view. | Elements displaying sensitive data (SSN). |
| `data-cobrowsing-masked` | Value as asterisks. | Inputs holding sensitive data (password). |
| `data-cobrowsing-disabled` | Element and value, but cannot interact. | Controls an agent should not use (submit button). |

Triggering:

- **Page load**: automatic scan. Static widgets need nothing else.
- **Event**: for late-rendered content (Vue widgets), dispatch `conceal-elements`. The dispatching element is the root; only its children are processed. The listener is on `document`, so an event from a child element needs `bubbles: true`.
- **CustomEvent**: dispatch on `document` with `detail.element` (an `HTMLElement`) or `detail.selector` (first match in the document is used).

```
interface IMaskingCustomEvent { detail: { element?: HTMLElement; selector?: String; } }

document.dispatchEvent(new Event('conceal-elements'));                                              // whole document
document.querySelector('.main').dispatchEvent(new Event('conceal-elements', { bubbles: true }));  // children of .main
document.dispatchEvent(new CustomEvent('conceal-elements', { detail: { element: document.querySelector('.main') } }));
document.dispatchEvent(new CustomEvent('conceal-elements', { detail: { selector: '.main' } }));
```

Sources: Conceal Elements, https://confluence.alkami.com/spaces/SDKC/pages/303531344; Global Components, https://confluence.alkami.com/spaces/SDKC/pages/303531343

### 9.2 Flash Banner (Alkami.FlashBanner)

Temporary status banner across the top of the screen. All methods return `void`.

- `show(message, type)` where `type` is `'bulletin' | 'caution' | 'error' | 'message' | 'info' | 'success' | 'warning'`.
- Shortcuts: `showBulletin(message)`, `showCaution(message)`, `showWarning(message)` (caution/warning), `showError(message)`, `showMessage(message)`, `showInfo(message)` (message/info), `showSuccess(message)`.
- `hide()`: hides the visible banner before its timer fires; otherwise no-op.
- `flashBanner(messageElement: HTMLElement, bannerOptions: IFlashBannerOptions)`: detailed control. Defaults: success banner, 6 seconds, attached to `#meta_header`; your options are merged in.

```
interface IFlashBannerOptions {
  attachToElement?: HTMLElement;    // element the banner is attached to
  flashBannerDisplayTime?: number;  // ms
  bulletin?: boolean; caution?: boolean; error?: boolean; message?: boolean;
  info?: boolean; success?: boolean; warning?: boolean;
}
```

Sources: Flash Banner, https://confluence.alkami.com/spaces/SDKC/pages/303531345

### 9.3 Mobile Titlebar and window.nativeHook

The mobile titlebar sits at the top of each mobile page (opens the nav drawer, widget actions). The page itself warns the component "is not where we want it to be in terms of quality and ease of use" and that improvements are coming. Two implementations exist and a widget must handle both:

- **Web (non-native)**: HTML rendered by the widget from a custom Razor section named `Header`.
- **Native app**: the HTML titlebar is hidden and the native titlebar is configured through the `window.nativeHook` JavaScript bridge.

#### HTML titlebar (index.cshtml)

```
@section Header {
<div class="titlebar">
<!-- Left side buttons -->
<a class="titlebar-button left" href="#">Nav</a>
<a class="titlebar-button left icon back hidden" href="#"></a>
<a class="titlebar-button left hidden" href="#">Cancel</a>
<!-- Title -->
<h1 id="page_title" class="truncate">ACH</h1>
<!-- Right side buttons -->
<a class="titlebar-button right hidden" href="#">Done</a>
</div>
}
```

Rules: the `Header` section renders only in a non-native environment; toggling the `hidden` class is the widget's job. Buttons must exist on page load or be added via JS. Every button needs class `titlebar-button`, plus `left` or `right`. For an icon add class `icon` and one of `back`, `home`, `nav` (default is the hamburger nav icon); without `icon` the inner text is shown. The title must be inside an `h1` (`id="page_title"`); class `truncate` is recommended.

Height: the web titlebar is 45px and content gets a 20px gap, so `#wrapper div#content` has `margin-top: 65px !important` on web and `20px` when the body has class `.native-menu` (added inside the native app). Add `.flush-with-titlebar` to a view (`margin-top: -20px`) to sit flush under the titlebar.

#### Native titlebar (window.nativeHook)

**Call `setNavBar` on page load** (inline in the mobile view's `BodyScripts` section or in widget JS), then `updateNavBar` for all later changes.

```
@section BodyScripts {
<script>
window.nativeHook.setNavBar({
  text: 'Disclosure',
  left: { button: 'back', callback: 'window.location.href = "\Mobile\Disclosures"' },
  right: { button: 'custom', text: 'Agree', callback: '$("form#mobile_disclosure").submit()' }
});
</script>
}

window.nativeHook.updateNavBar({ text: 'Widget Title', left: { button: 'drawer' } });
```

```
interface ITitlebarOptions {
  left?: ITitlebarButton;  // usually drawer or back
  right?: ITitlebarButton;
  text: string;            // title shown in the native titlebar
}
interface ITitlebarButton {
  button: string;    // 'back' | 'custom' | 'drawer' | 'home'
  text?: string;     // If 'custom', use this text instead of an icon
  callback?: string; // String containing JS that will be invoked with eval()
}
```

`window.nativeHook` API: `isAndroid()` (Boolean), `isNativeApp()` (Boolean; user agent has the `nativeapp` suffix), `isNativeAppWithNativeMenu()` (Boolean; `nativemenu` suffix; always equals `isNativeApp()`), `getNavBar()` (stored `ITitlebarOptions` object), `getNavBarString()` (stringified), `setNavBar(navBarJson)` (initial), `updateNavBar(navBarJson)` (subsequent).

```
if (window.nativeHook.isNativeApp()) { window.open(data.SsoUri, "_self"); } else { window.open(data.SsoUri, "_blank"); }
```

#### Vue titlebar example

A Vue 2 component (`Vue.extend({ name: 'titlebar', ... })`) from an Alkami Vue app drives both implementations. `INavBar` and `NavAction` (`drawer`, `back`, `custom`, `close`) are app-local interfaces. Props: `leftButton` and `rightButton` (`Object as () => NavAction`, default `null`), `title` (String), `transparent` (Boolean); all four are watched and call `updateTitleBar`. Data: `firstUpdate: true`, `isNative: window.nativeHook.isNativeApp()`. In `mounted()` it injects `<a class="titlebar-button left hidden titlebar-button--left">` (prepend) and `<a class="titlebar-button right hidden titlebar-button--right">` (append) into `.titlebar`, then calls `updateTitleBar()`. The HTML branch sets `#page_title` text, adds `hidden` to every `.titlebar-button`, then un-hides `#nav_button` for `drawer`, `#btnBack` for `back` (with `href = 'javascript:' + callback`), fills the injected anchors' text and href for `custom`, and adds `font-icon-cancel-x` for `close`.

```
updateTitleBar() {
  if (this.isNative) {
    const updateNavBar = this.firstUpdate ?
      (window as any).nativeHook.setNavBar : (window as any).nativeHook.updateNavBar;
    updateNavBar({ left: this.leftButton, right: this.rightButton, text: this.title });
    this.firstUpdate = false;
    return;
  }
  // Otherwise, work with the HTML titlebar through JS
  const titleBarElement = document.querySelector('.titlebar');
  if (!titleBarElement) { return; }
  document.getElementById('page_title').innerText = this.title || '';
  (Array.from(titleBarElement.querySelectorAll('.titlebar-button')) as HTMLElement[])
    .forEach((element) => element.classList.add('hidden'));
  // ... switch on this.leftButton.button / this.rightButton.button and un-hide the matching element ...
}
```

Sources: Mobile Titlebar, https://confluence.alkami.com/spaces/SDKC/pages/303531347; Vue Titlebar Example, https://confluence.alkami.com/spaces/SDKC/pages/303531350

### 9.4 Widget Header (Alkami.WidgetHeader)

The Widget Header is composed of a **Title**, **Navigation** (tabs to the widget's branches of functionality), and **Actions** (widget-wide actions, for example "Add Payee" in Bill Pay). In the simplest case you never touch it; it self-initializes from system content fetched from the server and cached in session storage under the widget name.

Rules:

- Prefer the **events** over the global `Alkami.WidgetHeader` instance. `widgetHeaderInitBefore` and `widgetHeaderInitAfter` fire on the header element (`Alkami.WidgetHeader.headerElement`) and bubble to `document`; `event.detail` is the `WidgetHeaderService` instance.
- **Register these listeners outside any ready function or `DOMContentLoaded` handler** or you may miss them.
- Use `widgetHeaderInitBefore` to add action items or set `preventDefaultFetch`; use `widgetHeaderInitAfter` (header fully populated) to set the title, add navigation, or touch system action items such as the Articles action item.

```
// Title and navigation (after init)
document.addEventListener('widgetHeaderInitAfter', function(event) {
  const service = event.detail;
  service.header.title = 'My Custom Title';
  service.header.navigation.append(new service.NavigationItem({ title: 'Custom Tab', url: 'Widget/Custom/Path' }));
});

// Action item (before init; actions need nothing from header info)
document.addEventListener('widgetHeaderInitBefore', function(event) {
  const service = event.detail;
  const actionItem = new service.ActionButtonItem({ iconName: 'dollar', buttonId: 'money_making_action_item' });
  actionItem.element.addEventListener('widgetActionItemActivated', function(event) { /* handle click */ });
  service.header.actions.append(actionItem);
});

// Prevent the System Content pull (e.g. an unregistered SSO widget that iframes content)
document.addEventListener('widgetHeaderInitBefore', function(event) {
  event.detail.preventDefaultFetch = true;   /* then update the header yourself */
});
```

**SPA usage.** Set `preventDefaultFetch = true` and your own title in `widgetHeaderInitBefore`, then in `widgetHeaderInitAfter` append each route as `new NavigationItem(Object.assign({ preventClick: true }, route))`. `preventClick: true` avoids a full page refresh and enables two events on `navigation.element`: `widgetNavigationItemActivated` (tab clicked or key-activated; build UI, then set `event.detail.navigation.selectedItem = event.detail.navigationItem`) and `widgetNavigationItemSelected` (fires when `navigationItem.selected = true`; display the view and start async calls there).

**API reference**

- **WidgetHeaderService** (`Alkami.WidgetHeader`). Events `widgetHeaderInitBefore`, `widgetHeaderInitAfter` (`event.detail` is the service). Constructors: `ActionButtonItem` (preferred), `ActionItem` (advanced), `NavigationItem`. Properties: `currentWidgetName` (string; usually the Area folder name; cache key), `header` (`WidgetHeader`), `headerId` (string; `document.getElementById(Alkami.WidgetHeader.headerId)`), `preventDefaultFetch` (boolean, default `false`). Functions: `fetchHeaderInfo()` returns `Promise<WidgetHeaderInfo>`; `storeHeaderInfo(widgetHeaderInfo)`; `resetHeaderInfoStorage()`; `initHeader()` (runs when DOM is ready).
- **WidgetHeader** (`Alkami.WidgetHeader.header`): `actions` (`WidgetActions`), `element`, `navigation` (`WidgetNavigation`), `title` (string get/set; custom HTML not supported).
- **WidgetNavigation**: events `widgetNavigationItemActivated`, `widgetNavigationItemSelected` (`event.detail.navigation`, `event.detail.navigationItem`). Properties `element`, `items` (immutable), `length`, `selectedItem` (get/set). Functions `append(item, ...)`, `prepend(item, ...)`, `insert(item, index)`, `remove(item)`, `getItem(index)`, `getItemIndex(item)`, `refreshUI(rebuildList: boolean)`.
- **WidgetNavigationItem** (`new NavigationItem(options)`, also `Alkami.WidgetHeader.NavigationItem`). Required: `title`, `url` (must be unique, or set `id`). Optional: `id` (derived from url), `preventClick`. Properties `element`, `id`, `linkElement`, `selected` (get/set), `title` (get/set, no HTML).
- **WidgetActions**: `element`, `items`, `length`; `append`, `prepend`, `insert(item, index)`, `remove`, `getItem(index)`, `getItemIndex(item)`, `refreshUI()`.
- **WidgetActionButtonItem** (`new ActionButtonItem(options)`, also `Alkami.WidgetHeader.ActionButtonItem`; extends WidgetActionItem). Event `widgetActionItemActivated` on its element (`event.detail.actionItem`). Options: `buttonId`, `containerId`, `href` (link-only button), `iconName` (Iris icon class minus `font-icon-`), `text`, `buttonAttributes` (e.g. `{'title': 'My Title', 'aria-label': 'label me please!'}`). Properties `buttonElement`, `buttonId`, `containerElement` (= `element`), `containerId` (= `id`), `visible` (get/set).
- **WidgetActionItem** (`new ActionItem(element: HTMLElement)`, also `Alkami.WidgetHeader.ActionItem`): thin wrapper for a custom element; properties `element`, `id`, `visible`. Prefer `ActionButtonItem`.

Sources: Widget Header, https://confluence.alkami.com/spaces/SDKC/pages/303531351

## 10. Supported Third Party Libraries: Chart.js

Chart.js is the standard chart library in the Alkami ecosystem (examples reference the 3.5.1 docs, https://www.chartjs.org/docs/3.5.1/). Runtime files are served from the platform under `~/lib/`; load them with `@Html.ScriptWithCacheExpiration` in the view's scripts section. Give every `canvas` `role="img"` and a meaningful `aria-label`, updated when data changes.

```
@* core chart library *@
@Html.ScriptWithCacheExpiration("~/lib/chartjs/chart.min.js")
@* If using Time Scales in chart.js then add the following *@
@Html.ScriptWithCacheExpiration("~/lib/chartjs-adapter-date-fns/chartjs-adapter-date-fns.bundle.min.js")
@* The Vue extension for chart.js (only when using vue-chartjs) *@
@Html.ScriptWithCacheExpiration("~/lib/vue-chartjs/vue-chartjs.min.js")
```

Plain JavaScript (time-scale line chart; markup is `<div style="width: 500px; height: 250px"><canvas id="myChart" role="img" aria-label="..."></canvas></div>`):

```
const canvasElement = document.getElementById('myChart');
const myChart = new Chart(canvasElement.getContext('2d'), {
  type: 'line',
  data: {
    datasets: [{
      label: 'Account Balance',
      data: randomizeData(today),            // array of { x: Date, y: number }
      backgroundColor: 'rgb(0, 113, 229)',   // point fill
      borderColor: 'rgb(0, 113, 229)',       // line and point border
      borderWidth: 1,
      fill: { target: 'origin', above: 'rgba(0, 113, 229, .5)' }, // fill under the line
    }]
  },
  options: {
    plugins: { legend: { display: false } },   // use HTML for the legend
    scales: {
      x: { type: 'time',                       // requires chartjs-adapter-date-fns
           display: true, ticks: { maxTicksLimit: 5 }, grid: { display: false, drawBorder: false } },
      y: { beginAtZero: true, grid: { drawBorder: false },
           ticks: { maxTicksLimit: 5, callback: function(value, index, values) { return '$' + value; } } }
    }
  }
});

// Update data and the aria label, then redraw
myChart.data.datasets[0].data = data;
canvasElement.setAttribute('aria-label', 'account balance history values in dollars per date. ' + describe(data));
myChart.update();
```

(The source snippet's update block has typos, `const currencyFormatter = var formatter = ...` and a one-argument `chartElement.setAttribute(...)`; it intends to build the label from `Intl.DateTimeFormat('en-US')` and `Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })` per point.)

### Chart.js with Vue (vue-chartjs)

- Use the `vue-chartjs` wrapper only when chart data is simple (just values). For custom per-element data (labels, amounts, extra fields per item) use Chart.js directly; the wrapper does not support that.
- Install npm packages **for types only**; webpack pulls them out because the runtime scripts come from `~/lib/`: `npm install chart.js chartjs-adapter-date-fns vue-chartjs`
- Extendable components: `Bar`, `Bubble`, `Doughnut`, `HorizontalBar`, `Line`, `Pie`, `PolarArea`, `Radar`, `Scatter`. Guide https://vue-chartjs.org/guide/, API https://vue-chartjs.org/api/.

Doughnut.vue (no template; swap `Doughnut` and `options` for other chart types):

```
<script lang="ts">
import Vue, { PropOptions } from 'vue';
import { Doughnut, mixins } from 'vue-chartjs';
import { ChartData, ChartOptions } from 'chart.js';
const { reactiveProp, reactiveData } = mixins;

export default Vue.extend({
  extends: Doughnut,
  mixins: [reactiveProp, reactiveData],
  data() {
    return { options: { cutout: '85%', responsive: true, maintainAspectRatio: true,
                        plugins: { legend: { display: false } } } as ChartOptions };
  },
  props: {
    chartData: { type: Object, required: true } as PropOptions<ChartData>,
    label: { type: String, required: true },
  },
  watch: { label() { this.setLabel(this.label); } },
  methods: {
    chart() { return this as unknown as Doughnut; },
    setLabel(label: string) {
      const canvas = this.chart().$refs.canvas as HTMLCanvasElement;
      if (canvas) { canvas.setAttribute('role', 'img'); canvas.setAttribute('aria-label', label); }
    },
  },
  mounted() { this.chart().renderChart(this.chartData, this.options); },
});
</script>
```

Use it from a container component as `<doughnut-component :chart-data="chartdata" :label="label" />`, where `chartdata` is a Chart.js data object (`labels: [...]`, `datasets: [{ label, data: [...], backgroundColor: [...] }]`). Mutating `chartdata.datasets[0].data[0]` re-renders through the reactive mixins. The page's example palette: `#BCC3C7` grey, `#CFEFB3` sage, `#6DC6A3` green, `#009A9F` teal, `#006A97` navy, `#003A7C` blue, `#130047` violet, `#7A1155` purple, `#C24F57` peach, `#EE9B5F` orange, `#FFEC88` yellow. The page includes screenshots of the resulting charts that are not reproduced here.

Sources: Chart.js, https://confluence.alkami.com/spaces/SDKC/pages/303531354; Supported Third Party Libraries, https://confluence.alkami.com/spaces/SDKC/pages/303531353

## 11. Polyfills and JavaScript Standards

- **Supported ECMAScript version**: officially, as of platform release 2022.1, up to **ECMAScript 2019**.
- Other supported standards: WHATWG DOM (ParentNode and ChildNode mixins), WHATWG Fetch, WHATWG URL.
- **Polyfills provided** for supported browsers: `Array.prototype.flat`, `Array.prototype.flatMap`, `Array.prototype.values`, `DocumentFragment.prototype.append`, `DocumentFragment.prototype.prepend`, `Element.prototype.after`, `Element.prototype.append`, `Element.prototype.before`, `Element.prototype.inert`, `Element.prototype.prepend`, `Element.prototype.replaceWith`, `Element.prototype.toggleAttribute`, `Event.hashchange`, `globalThis`, `Object.entries`, `Object.fromEntries`, `Object.getOwnPropertyDescriptors`, `Object.values`, `Promise.prototype.finally`, `String.prototype.padEnd`, `String.prototype.padStart`, `String.prototype.replaceAll`, `String.prototype.trim`, `String.prototype.trimEnd`, `String.prototype.trimStart`, `Symbol.asyncIterator`, `URL`.
- **Transpiled-only** (TypeScript is the supported and recommended transpiler): optional chaining, null coalescing, logical assignment operators, `async`/`await`, imports, decorators (discouraged; current decorator proposals differ from what TypeScript implements and are not Stage 3).
- **Avoid in browser code**: `Promise.race()`, `Promise.allSettled()`, `Promise.any()` (support varies and polyfills were too large to ship on every page); smooth scrolling options on scroll functions such as `scrollIntoView` (poorly supported, can throw). Newer ECMAScript features in general have questionable support; transpile with TypeScript if needed.

Sources: Polyfills and JavaScript Standards, https://confluence.alkami.com/spaces/SDKC/pages/303531359; Front End API, https://confluence.alkami.com/spaces/SDKC/pages/303531336
