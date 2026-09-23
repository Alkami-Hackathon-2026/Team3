# Vue Project Guide (Alkami Client Developer Network)

**What this covers.** The Alkami-recommended way to structure, build, and test a Vue widget on the Alkami digital banking platform: the folder anatomy of a Vue project (API layer, `app.ts`, `App.vue`, components, Pinia store, directives, interfaces, router, styles, utils, views), unit/component testing with Jest and `@vue/test-utils`, end-to-end testing with Cypress and a MirageJS mock server driven by Albus, a handful of tips (typed props, deep CSS selectors, initial data fetching), and the Vue 2 to Vue 3 migration guide. This is not an introduction to Vue itself. Vue 2 has been deprecated at Alkami in favor of Vue 3; for Vue 2 practices consult older widgets or https://v2.vuejs.org/. Note that several sample files on the source pages (the component-test examples in particular) still use the Vue 2 Options API (`Vue.extend`, `propsData`, `filters`); this is called out where it occurs.

Recommended tooling: VS Code with the Volar extension (https://marketplace.visualstudio.com/items?itemName=Vue.volar) and the Vue Devtools browser extension (https://devtools.vuejs.org/).

Sources: Vue (https://confluence.alkami.com/spaces/SDKC/pages/303531360), 1. Introduction (https://confluence.alkami.com/spaces/SDKC/pages/303531361)

## 1. Anatomy of a Vue project

### Folder structure

The project lives inside the widget's client folder (referred to as `<projectNamespace>` in Albus config). The Vue source is under `Scripts/`.

```
Scripts/
  api/                  # Endpoint communication. index.ts barrel + users.ts, accounts.ts ...
  components/           # All non-route Vue components, grouped by folder (form/, modal/, shared/ ...)
                        # Component file names are PascalCase (TextInput.vue, AccountDropdown.vue)
  directives/           # Custom Vue directives (focus.ts ...) imported in app.ts
  interfaces/           # One exported TypeScript interface per file (IBaseResponse.ts, IPayee.ts, IAccount.ts)
  store/                # Global store. store.ts, plus optional modules/ (activity.ts, payment.ts) for large projects
  styles/               # Global scss shared across the app. Variables ONLY (_animation.scss, _colors.scss)
  utils/                # Utility functions, each file exports what it shares (localStorage.ts, userStorage.ts, dom.ts)
  views/                # Route components, 1:1 with router routes (Dashboard.vue, Manage.vue, Activity.vue)
  app.ts                # Main entry point, imports App.vue
  App.vue               # Main application Vue component
  router.ts             # Routes and routing related code
  shims-vue.d.ts        # TypeScript typings for .vue modules
server/                 # MirageJS mock server for albus-module-cypress e2e testing
  index.ts              # Contains createMockServer, which specifies mock server routes
  mocks/                # Service response mock data
    entities/accounts.ts    # Mock data entities for API responses
    accountScenario.ts      # Named the same as the E2E scenario in tests/e2e/specs
    errorScenario.ts
    index.ts                # Barrel file exporting its siblings
tests/                  # Jest setup files and utility scripts
  globalSetup.js        # Optional
  e2e/
    modules/accountInputTests.js   # Named the same as its scenario with "Tests" suffix
    plugins/index.js               # File and folder configuration referenced by cypress.json
    specs/accountInputScenario.js  # Each scenario lives separately from its tests
    support/commands.js            # Extra commands for Cypress
    support/index.js
  mocks/                # Data mocks for unit tests (payees.ts, accounts.ts ...)
  unit/
    components/TextInput.spec.ts   # Named the same as the component with ".spec" suffix
    utils/translation.spec.ts      # Named the same as the source file with ".spec" suffix
.browserslistrc         # Browsers supported, used in SCSS compilation
.npmrc                  # Defines the Alkami npm feed for internal dependencies
albus.config.js         # Albus configuration. See albus-preset-vue docs
cypress.json            # E2E configuration. See albus-module-cypress docs
jest.config.js          # Optional, only to override Albus defaults
package.json            # See albus-preset-vue for required dependencies
postcss.config.js       # CSS config for SCSS compilation
tsconfig.json           # TypeScript configuration
```

Folder roles in brief:

- `api`: every API request, each exported so it can be imported individually or all at once.
- `components`: all non-route components, grouped by folder as makes sense.
- `directives`: custom Vue directives, registered globally or per component.
- `interfaces`: one exported TypeScript interface per file.
- `router.ts`: the Vue Router definition.
- `store`: the Pinia store (the page text still says "VueX" in places; the code samples use Pinia). Single file for small projects, modules for larger ones.
- `styles`: global stylesheets. Only variable files or styles for third party libraries.
- `server`: MirageJS service mocks for end-to-end scenarios (`albus-module-cypress`).
- `tests`: Jest unit tests and Cypress e2e tests.
- `utils`: basic utility functions (for example a local storage manager).
- `views`: main pages, mapping 1:1 to router route definitions.
- `app.ts` (the page also calls it `Main.ts`): Webpack entry point; this is the script ultimately referenced from the widget's `index.cshtml`.
- `App.vue`: the root component mounted by `app.ts`.

Sources: 2. Anatomy of a Vue Project (https://confluence.alkami.com/spaces/SDKC/pages/303531362)

### API (request layer)

All functions that call an API live in `Scripts/api`. Each request is an exported `async` function returning a Promise. Import only the functions you need; Webpack produces smaller bundles when files import only what they use.

```ts
import * as Requests from './Scripts/api/requests'; // DON'T: Imports everything
import { getTenants } from './Scripts/api/requests'; // DO: Only imports the method needed
```

Example `requests.ts`:

```ts
// GET Requests
// =============================================================================
export async function getTenants(): Promise<ITenant[]> {
  const response = await fetch('/api/tenants');

  if (response.ok) {
    const tenantsResponse: ITenantResponse = await response.json();

    if (!tenantsResponse.hasError) {
      // Only return the itemList since we know the request was successful.
      return tenantsResponse.itemList;
    }

    throw new Error(consolidateValidationMessages(tenantsResponse));
  }

  throw new Error(response.statusText);
}

// POST Requests
// =============================================================================
export async function authenticateAsUser(request: IUserAuthRequest): Promise<{ url: string, syncCompleted: boolean, hasError: boolean }> {
  const fetchOptions = {
    method: 'POST',
    body: JSON.stringify(request),
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const response = await fetch('/api/loginasuser', fetchOptions);

  if (response.ok) {
    const loginResponse: IUserAuthResponse = await response.json();
    return ({ url: loginResponse.url, syncCompleted: loginResponse.syncCompleted, hasError: loginResponse.hasError });
  }

  throw new Error(response.statusText);
}
```

Usage inside a component is a plain import (`import { authenticateAsUser } from '../api/requests';`) followed by `const authPayload = await authenticateAsUser(newPayload);` inside a method.

**Grouping requests.** When there are many requests, split them into files by resource (`users.ts`, `accounts.ts`) and add a barrel `index.ts` at the root of the API folder:

```ts
export * from './users';
export * from './accounts';
```

Then `import * as Requests from '../Api/index';` exposes every exported function on the `Requests` object.

Sources: API (https://confluence.alkami.com/spaces/SDKC/pages/303531363)

### app.ts (application entry point)

The entry point is what Webpack starts from. It imports Vue and the root component, registers the router, store and other plugins, prepares mock data when running under Cypress, and mounts the app. The `vue` import is stripped by Webpack and Vue must be included as a script tag in `index.cshtml`.

```ts
import { createPinia } from 'pinia';
import { createApp } from 'vue';
import App from './App.vue';
import router from './router'; // Optional: If using the router, import the main file here.

void (async () => {
  const pinia = createPinia()

  const app = createApp(App);
  app.use(pinia);
  app.use(router);

  if (process.env.NODE_ENV === 'development' && process.env.ALBUS_LOCAL_DEV === 'cypress') {
    // Setup mock data if needed. This code will tree shake out via replacement of variables above.
    const { setup } = await import('./tests/e2e/setup')
    await setup();
  }

  app.mount('#app');
})();
```

The app must mount into an element with id `app`. Albus e2e testing will not render the application if any other id is used.

Sources: App.ts (application entry point) (https://confluence.alkami.com/spaces/SDKC/pages/303531364), 4. End-to-end Testing a Vue Project (https://confluence.alkami.com/spaces/SDKC/pages/303531391)

### App.vue (main application view)

The root component sets the initial HTML structure, manages global loading state, registers `router-view` outlets, and kicks off initial data requests. Its `<style>` block is the only one in the application that should be unscoped.

```vue
<template>
  <div class="app">
    <NavBar />
    <router-view name="page" />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useStore } from './store';
import { logoutUser } from './api/requests';

// Child Components
import NavBar from './components/NavBar.vue'

const store = useStore();

onMounted(() => {
  store.getInitialData();
});
</script>

<style lang="scss"> // <= This is the only style tag that should be unscoped
// Import any third party libraries here
@import './styles/_snotify-custom.scss';

.app {
  display: flex;
  flex-direction: column;
  height: 100%;
}

// Any additional global styles can go here
</style>
```

Sources: App.vue (main application view) (https://confluence.alkami.com/spaces/SDKC/pages/303531365)

### Components

Components not tied to a route live in `Scripts/components`. Every component is a single file component with `<script setup lang="ts">`, `<style lang="scss" scoped>`, and standard HTML in the template.

Template conventions:

- Write accessible HTML (correct `role`, `tabindex`, aria attributes, keyboard handlers).
- Only one element at the root of a template.
- Put each attribute on its own line when there are many; group plain HTML attributes first, then Vue attributes.

```vue
<template>
  <div class="tenant-card">
    <div
      class="iris-card iris-card--shadow"
      data-hoverable
      role="button"
      tabindex="0"
      @click="selectTenant"
      @keydown.prevent.stop.enter.space="selectTenant"
    >
      <div class="tenant-card__header iris-card__header">
        <h4 class="tenant-card__title iris-card__title truncate">
          {{ props.tenant.name }}
        </h4>
        <div class="tenant-card__url truncate">
          {{ props.tenant.bankUrlSignatures[0] }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from '@vue/reactivity';
import { defineProps } from 'vue';
import { ITenant } from '../interfaces/ITenant';
import router from '../router';
import { useStore } from '../store';

const store = useStore();

// Type your props for your Component
interface Props {
  tenant: ITenant;
}
// DO NOT DESTRUCTURE PROPS, they will loose their reactivity
const props = defineProps<Props>();

async function selectTenant() {
  selectedTenant.value = props.tenant;
  await router.push('explore');
}

// computed property to utilize the store
const selectedTenant = computed({
  get: () => {
    return store.selectedTenant;
  },
  set: (newValue: ITenant) => {
    store.setSelectedTenant(newValue);
  },
});
</script>

<style lang="scss" scoped>
.tenant-card {
  cursor: pointer;
  margin-bottom: 30px;
}
</style>
```

Script conventions: `lang` is always `ts`; group interface imports and child component imports; always give a return type for a computed property; type props with an interface passed to `defineProps<Props>()`; never destructure `props`. Style conventions: `lang="scss"` and `scoped` always.

Sources: Components (https://confluence.alkami.com/spaces/SDKC/pages/303531366), Complex Interface for a Prop (https://confluence.alkami.com/spaces/SDKC/pages/303531398)

### Data store (Pinia)

Rule: if a piece of data is shared by more than one component it goes in the store, otherwise it stays local to the component. A store is `state`, `getters`, and `actions`:

```ts
import { defineStore } from 'pinia';

const appStore = defineStore('app', {
  state: () => ({}), // Any data points to be shared across components
  getters: {},       // Like computed properties for state; returns a subset of the state.
  actions: {},       // Methods that ultimately end in mutating state.
});
```

**State.** All values must be declared with an initial value; properties cannot be added after initialization. Use type casts or an interface for TypeScript support. Alphabetizing is the recommended ordering, logical grouping is acceptable.

```ts
state: () => ({
  availableTenants: [] as ITenant[],
  colorScheme: 'light',
  hasLoadedInitialData: false,
  selectedTenant: {} as ITenant,
}),

// or use an interface
interface IMyAppStore {
  availableTenants: ITenant[];
  colorSchema: string;
  hasLoadedInitialData: boolean;
  selectedTenant: ITenant | null;
}

state: (): IMyAppStore => ({
  availableTenants: [],
  colorSchema: 'light',
  hasLoadedInitialData: false,
  selectedTenant: null,
}),
```

**Actions.** Actions are the only thing that should modify state. They are like component methods and may be sync or async; the common pattern is call the API, then set state via `this`.

```ts
actions: {
  updateColorScheme(payload: string) {
    switch (payload) {
      case 'dark':
        this.colorScheme = 'dark';
        break;
      default:
        this.colorScheme = 'light';
        break;
    }
  },
  updateLoadingState(payload: boolean) {
    this.hasLoadedInitialData = payload;
  },
},
```

(The source page's actions block also contains older Vuex-style signatures such as `clearSelectedTenant(state)` and `getInitialData(context)` using `context.commit(...)`. Those are leftovers from the Vuex version of the page; in Pinia, actions mutate `this` directly and call other actions as methods. See the `getInitialData` example under Fetching Initial Data below for the Pinia form.)

**Getters.** Synchronous filters over state, optionally parameterized by returning a function.

```ts
getters: {
  favoriteTenants(state) {
    return state.availableTenants.filter((tenant) => tenant.isFavorite);
  },
  filteredTenants: (state) => (query: string) => {
    return state.availableTenants.filter((tenant) => {
      if (!query) {
        return true;
      }
      const lowerName = tenant.name.toLowerCase();
      const lowerUrl = tenant.bankUrlSignatures[0].toLowerCase();
      const lowerQuery = query.toLowerCase().trim();
      return (lowerName.indexOf(lowerQuery) > -1) || lowerUrl.indexOf(lowerQuery) > -1;
    });
  },
},
```

**Making the store available.** Export the store from `store.ts`, name it `use<Name>Store`, and register Pinia on the app in `app.ts` with `app.use(pinia)` (see app.ts above).

```ts
import { defineStore } from 'pinia'

// Name the return value use + Name + Store (e.g. useUserStore, useCartStore).
// The first argument is a unique id of the store across your application.
export const useStore = defineStore('main', {
  // other options...
})
```

**Using the store in a component.** Call `useStore()` in `<script setup>`, read getters as properties, call actions as methods. Use actions to mutate the store and computed properties to read it.

```vue
<template>
  <div class="flex flex--wrap pad-y--xl">
    <iris-chip
      v-for="tenant in store.filteredTenants"
      :key="tenant.id"
      :label="tenant.name"
      :name="'label' + tenant.id"
      class="mar-right--lg"
    />
  </div>
</template>
<script setup lang="ts">
import { useStore } from './store';
import { getFilters } from './api'

const store = useStore();

function getTenantById(id): ITenant {
  return store.filteredTenantsById(id); // getter with a parameter
}
async function fetchPageData() {
  store.getInitialData(); // invoke an action
}
async function fetchAvailableFilters() {
  const filters = await getFilters(store.selectedTenant.bankIdentifiers[0]);
  store.populateAvailableFilters(filters); // action that sets state
}
</script>
```

Sources: Data Store (https://confluence.alkami.com/spaces/SDKC/pages/303531367)

### Directives

Custom directives are for reusable logic that needs low-level DOM access on plain elements. Each file in `Scripts/directives` exports a single directive.

```ts
// focus.ts: a custom directive called v-focus
export default (element: HTMLElement) => {
  element.focus();
};
```

Register globally in `app.ts` with `app.directive('focus', focus)` (first argument is the directive name), or locally in a component by importing with the `vName` convention:

```vue
<template>
  <input v-focus />
</template>

<script setup lang="ts">
import vFocus from '../directives/focus';
</script>
```

Sources: Directives (https://confluence.alkami.com/spaces/SDKC/pages/303531368)

### Interfaces

`Scripts/interfaces` holds one exported interface per file. Rules:

- All interfaces must begin with a capital `I` and be PascalCase (`IBaseTransaction`, `ITransaction`).
- The filename must equal the interface name (`ITransaction.ts`).
- Each file exports exactly one member. Helper interfaces used only to compose the exported one may be declared (unexported) in the same file.
- Interface files may import other interfaces to extend them.

```ts
// IBaseTransaction.ts
export interface IBaseTransaction {
  categoryId?: number;
  categoryName?: string;
  minAmount: number;
  maxAmount: number;
  generalDescription: string;
  specificDescription: string;
  type?: string;
}

// ITransaction.ts
import { IBaseTransaction } from './IBaseTransaction';

export interface ITransaction extends IBaseTransaction {
  amount: number;
  balance: number;
  displayDate: string;
  transactionId: number;
}
```

Sources: Interfaces (https://confluence.alkami.com/spaces/SDKC/pages/303531369)

### Router (vue-router)

`router.ts` holds every route. Routes match top to bottom, so order matters when paths share prefixes (`/payee` vs `/payee/info`). Only import the landing page views statically; lazy load everything else with `() => import('./views/Component.vue')` so startup does not parse code the user may never reach. The history base is computed from whether the widget is running under `/Mobile/`.

```ts
import { createRouter, createWebHistory } from 'vue-router';

// ONLY the entry point or global views should be imported in this way.
import Dashboard from '../views/Dashboard.vue';
import NavBar from '../components/NavBar.vue';

const routes = [
  {
    path: '/',
    name: 'dashboard',
    components: {
      navbar: NavBar,
      page: Dashboard,
    },
  },
  {
    path: '/project/:projectName',
    name: 'project',
    components: {
      navbar: NavBar,
      page: () => import('./views/Project.vue'), // <= Lazy loaded route
    },
  },
  // For any route that doesn't match explicitly, redirect back to the dashboard
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
];

// Determine if we are the mobile or the desktop widget
const isMobile = location.pathname.toLowerCase().startsWith('/mobile');
const router = createRouter({
  routes,
  history: createWebHistory((isMobile ? '/Mobile/' : '/') + 'WidgetName/'),
});

export default router;
```

Named `components` (`navbar`, `page`) correspond to `<router-view name="page" />` in `App.vue`.

Sources: Router (Vue-Router) (https://confluence.alkami.com/spaces/SDKC/pages/303531370), Views (https://confluence.alkami.com/spaces/SDKC/pages/303531373)

### Styles

Most CSS lives scoped in components. Global styling (typically for a third party library) goes in a file under `Scripts/styles` and is imported from the unscoped `<style lang="scss">` block in `App.vue`, for example `@import './styles/_snotify-custom.scss';`. The `styles` folder should contain only variable files or third party library styles.

Sources: Styles (https://confluence.alkami.com/spaces/SDKC/pages/303531371)

### Utils

`Scripts/utils` holds utility functions used by the store and request layer (for example local storage helpers). Related functions are grouped into one file and each is exported individually, the same pattern as the API layer.

```ts
// localStorage.ts
import { IUser } from '../interfaces/IUser';

type UserStorage = 'recentUsers' | 'favoriteUsers';

export function removeUserFromLocalStorage(storageKey: UserStorage, usernameToRemove: string) {
  return new Promise((resolve, reject) => {
    const usersStorage = window.localStorage.getItem(storageKey);
    if (!usersStorage) {
      reject('No users were found under that storage key.');
      return;
    }
    const clonedUserList: IUser[] = JSON.parse(usersStorage);
    // ... find the user, splice it out, write the list back, resolve()
  });
}

export function clearStorageKey(storageKey: string) {
  window.localStorage.removeItem(storageKey);
}
```

Sources: Utils (https://confluence.alkami.com/spaces/SDKC/pages/303531372)

### Views

`Scripts/views` contains the primary page components. Each file maps to exactly one route in `router.ts`; for routes `home` (`Home`) and `choose` (`() => import('./views/ChooseTenant.vue')`) the folder contains `Home.vue` and `ChooseTenant.vue`.

Sources: Views (https://confluence.alkami.com/spaces/SDKC/pages/303531373)

## 2. Unit and component testing (Jest)

Jest looks for test files under `tests/unit`, and every test file must contain `.spec.` in its name. Component tests go in `tests/unit/components/<Component>.spec.ts`, function tests in `tests/unit/<area>/<sourceFile>.spec.ts`. Shared mock data lives in `tests/mocks` (one exported object per file, named after its contents).

Philosophy: a component test is not a UI test and does not replace Selenium/Puppeteer style browser tests. Components compile to functions with an interface (props) and behaviors (methods triggered by events); tests exercise those in isolation.

Conventions:

- Mount with `shallowMount` from `@vue/test-utils` whenever possible; use `mount` only when the child tree must render.
- One `describe` per file, named for the file under test (`describe('EmptyState.vue', ...)`, `describe('Translation.ts', ...)`).
- Test case names start with an action verb.
- Put `expect` assertions at the bottom of the test function.

Note: the three component examples on the source pages are written against Vue 2 (`Vue.extend`, `propsData`, `filters`, `this.$store.state`). The structure carries over to Vue 3 with `@vue/test-utils` v2 (`props` instead of `propsData`, `global.mocks`/`global.stubs` instead of top-level `mocks`/`stubs`), but the pages have not been updated for that.

### Example 1: basic rendering

```ts
import { shallowMount } from '@vue/test-utils';
import EmptyState from '../../../../Scripts/components/global/EmptyState.vue';

describe('EmptyState.vue', () => {
  it('renders properly when props are passed in', () => {
    const wrapper = shallowMount(EmptyState, {
      propsData: {
        header: 'test header',
        message: 'test message',
      }
    });

    const title = wrapper.find('.iris-empty-state__title').text();
    const message = wrapper.find('.iris-empty-state__copy').text();

    expect(title).toBe('test header');
    expect(message).toBe('test message');
  });
});
```

### Example 2: interactions

Trigger a DOM event, then assert on the result.

```ts
import { shallowMount } from '@vue/test-utils';
import { projectRefs } from '../../data/projectRefs';
import { executions } from '../../data/executions';
import RepositoryRecord from '../../../Scripts/components/project/RepositoryRecord.vue';

const repositoryInfo = projectRefs.projects[0];
const storeMock = {
  $store: {
    state: {
      executionAggregate: {
        '31DAE330A77807FA058B7867E3BD02307539FF2E': [executions.executions[0]],
      },
    },
  },
};

describe('RepositoryRecord.vue', () => {
  it('expands on row click', () => {
    const wrapper = shallowMount(RepositoryRecord, {
      propsData: { repositoryInfo },
      mocks: storeMock,
      filters: {
        formatProjectName(input: string) { return input; },
      },
    });

    wrapper.find('.repository__row').trigger('click');

    const expandedElement = wrapper.find('.repository__details');
    expect(expandedElement.exists()).toBe(true);
  });
});
```

### Example 3: mocks

Anything on the component instance can be mocked. Recommended pattern: a factory function at the top of the file that returns a fully valid mock object; each test mutates a fresh copy.

```ts
import { shallowMount } from '@vue/test-utils';
import NavBar from '../../../Scripts/components/global/NavBar.vue';
import LoaderCircle from '../../../Scripts/components/global/LoaderCircle.vue';

const createValidMockObject = () => {
  return {
    stubs: ['router-link'], // Stub out the built-in router link component
    mocks: {
      $store: {
        state: {
          lastUpdateDate: '2042-01-01',
          hasFinishedBackgroundSync: true,
          hasLoadedInitialData: true,
        },
        getters: {
          findProjectByRepositoryName: () => { return { name: 'RepositoryName'} },
        },
      },
      $route: {
        params: {
          projectName: 'test project',
          repositoryName: 'test repository'
        },
      },
    },
    filters: {
      spacerize: (input: string) => input,
    },
  };
};

describe('NavBar.vue', () => {
  it('renders correctly when store and state variables exist', () => {
    const wrapper = shallowMount(NavBar, createValidMockObject());

    expect(wrapper.find('#team_name').text()).toBe('test project');
    expect(wrapper.find('#repository_name').text()).toBe('test repository');
    expect(wrapper.find('.app-nav__updated').contains(LoaderCircle)).toBe(false);
  });

  it('renders loading indicator if background sync has not finished', () => {
    const localMock = createValidMockObject();
    localMock.mocks.$route.params.repositoryName = '';
    localMock.mocks.$route.params.projectName = '';
    localMock.mocks.$store.state.hasFinishedBackgroundSync = false;

    const wrapper = shallowMount(NavBar, localMock);

    expect(wrapper.find('.app-nav__updated').contains(LoaderCircle)).toBe(true);
  });
});
```

### Mock data files

Mock data under `tests/mocks` follows the interfaces convention: one file, one export, typed with the project's interface. Store the entire API response rather than only the fields under test, so the same mock serves API layer tests and component tests.

```ts
import { IProjectRefsResponse } from '../../Scripts/interfaces/IProject';

export const projectRefs: IProjectRefsResponse = {
  "ncover_version_info": { "ncover": "5.5.4144.642", "json": "5.5" },
  "ellapsedTime": "n/a",
  "projects": [ /* full response objects */ ]
}
```

### Functional (utility) testing

Import the function, call it, assert the output. Async functions are fine.

```ts
import * as Translator from '../../../Scripts/utils/translation';
import { projectRefs } from '../../data/projectRefs';
import { executions } from '../../data/executions';

async function getProjectAggregate() {
  return await Translator.translateProjectRefsToAggregate(projectRefs.projects);
}

describe('Translation.ts', () => {
  it('translates project refs response to aggregate', async () => {
    const output = await getProjectAggregate();

    expect(Object.keys(output).length).toBe(1);
    expect(Object.keys(output)[0]).toBe('AccountIntelligence');
  });
});
```

Sources: 3. Testing a Vue Project (https://confluence.alkami.com/spaces/SDKC/pages/303531374), Component Testing (https://confluence.alkami.com/spaces/SDKC/pages/303531375), Example 1 (Basic) (https://confluence.alkami.com/spaces/SDKC/pages/303531376), Example 2 (Interactions) (https://confluence.alkami.com/spaces/SDKC/pages/303531377), Example 3 (Mocks) (https://confluence.alkami.com/spaces/SDKC/pages/303531378), Data (Mocks) (https://confluence.alkami.com/spaces/SDKC/pages/303531380), Functional Testing (https://confluence.alkami.com/spaces/SDKC/pages/303531390)

## 3. End-to-end testing (Cypress + MirageJS + Albus)

E2E tests are UI tests that run the widget "live" in a simulated page. Cypress drives the browser; MirageJS mocks the services. Three parts make up an e2e suite: Test Cases (simulate user behavior), Service Mocking (data the widget expects), and Scenarios (orchestrate the two).

### Prerequisites and running

Requires Albus, `albus-preset-vue3`, and `miragejs`. After adding `albus-preset-vue3` to the project:

```
npm install --save-dev @alkami/orb-layout-mock miragejs
npm install --save-dev albus-module-cypress
```

Run e2e tests with Albus (an `--orbVersion` is required; omit it once to have Albus print the available versions):

```
npx albus substantiate --e2e -C Alkami.Client.Widget.MyWidget
npx albus substantiate --e2e -C Alkami.Client.Widget.MyWidget --orbVersion 2022.6.0
```

The Vue app must mount into `#app`, and `app.ts` should load e2e setup only when `process.env.NODE_ENV === 'development' && process.env.ALBUS_LOCAL_DEV === 'cypress'` (see app.ts above).

**Multiple entries.** By default Albus includes an `app.ts` entry for both desktop and mobile. `albus.config.js` `presetConfiguration.vue3.pluginConfiguration.routes` defines the paths served in the mock environment:

```js
module.exports = {
  global: {},
  tokens: {
    orbWidgetName: 'Example',
    clientFolderName: 'WebClient',
    areaFolderName: 'Areas'
  },
  presetConfiguration: {
    vue3: {
      pluginConfiguration: {
        webpack: {
          entry: {
            app: '<projectNamespace>/Scripts/app.ts',
          },
          outputPath: {
            js: '<projectNamespace>/Scripts',
            css: '<projectNamespace>/Styles',
          },
          autoInject: [
            {
              chunks: ['app'],
              htmlFilePath: '<projectNamespace>/Views/Index.cshtml',
              htmlTemplateFilePath: '<projectNamespace>/Views/Index.template.cshtml'
            },
            {
              chunks: ['app'],
              htmlFilePath: '<projectNamespace>/Views/Index.cshtml',
              htmlTemplateFilePath: '<projectNamespace>/Views/Mobile/Index.template.cshtml'
            },
          ],
          routes: [ // Routes created inside the mock environment
            {
              path: '/',        // Route path in the browser
              entry: 'app',     // What entry to add to the mock
              layout: 'desktop', // Which mock layout to use: 'desktop' or 'mobile'
            },
            {
              path: '/Mobile/',
              entry: 'app',
              layout: 'mobile',
            },
          ],
        }
      }
    }
  }
}
```

Sources: 4. End-to-end Testing a Vue Project (https://confluence.alkami.com/spaces/SDKC/pages/303531391)

### Step 1: bootstrapping Cypress

Four files: `cypress.json`, `tests/e2e/plugins/index.js`, `tests/e2e/support/index.js`, `tests/e2e/support/commands.js`.

```json
{
  "pluginsFile": "tests/e2e/plugins/index.js",
  "baseUrl": "http://localhost:8080"
}
```

`pluginsFile` should always be under `tests/e2e/plugins`; `baseUrl` is the MirageJS mock server URL.

```js
// tests/e2e/plugins/index.js
module.exports = (on, config) => {
  return Object.assign({}, config, {
    fixturesFolder: 'tests/e2e/fixtures',
    integrationFolder: 'tests/e2e/specs',
    screenshotsFolder: 'tests/e2e/screenshots',
    videosFolder: 'tests/e2e/videos',
    supportFile: 'tests/e2e/support/index.js'
  });
};
```

`integrationFolder` (where scenarios live) and `supportFile` are the important ones. The screenshot and video folders are output artifacts and must be added to `.gitignore`:

```
# Directory for Cypress test output
**/e2e/videos
**/e2e/screenshots
```

The support file runs before every spec and imports custom commands:

```js
// tests/e2e/support/index.js
import './commands';
```

```js
// tests/e2e/support/commands.js
Cypress.Commands.add(
  'selectOption',
  { prevSubject: 'element' },
  (subject, optionNumber = 0) => {
    const dropdownComponent = Alkami.Iris.DropdownComponent.componentForElement(subject);

    if (instance) {
      dropdownComponent.value = dropdownComponent.options[0].value;
    }
  });
```

Sources: 1. Bootstrapping Cypress (https://confluence.alkami.com/spaces/SDKC/pages/303531393)

### Step 2: local dev server with MirageJS

`server/index.ts` exports `createMockServer`, which both renders the widget and mocks API routes. Most of the wiring is provided by `albus-module-cypress`.

```ts
import { Server, Response } from 'miragejs';
import * as MOCKS from './mocks';

export function createMockServer(options: { environment: string }) {
  return new Server({
    environment: options.environment,

    routes() {
      this.namespace = '/StandardSSO';

      // Routes
      // =============================================================================
      /* Render and Mock API routes go here */
    },
  });
}
```

Mirage is pedantic about URL matching: `namespace` + route must match the requested URL exactly, including the leading slash. A mismatch produces an error such as `Mirage: Error: Your app tried to GET 'StandardSSO/GetData', but there was no route defined to handle this request.`

Sources: 2. Local Dev Server with MirageJS (https://confluence.alkami.com/spaces/SDKC/pages/303531394)

### Step 3: scenarios and test modules

A scenario has a Setup (spec file: URL to visit, viewport, other pre-test work) and a Module (the test cases), split into two files: `tests/e2e/specs/<name>Scenario.js` and `tests/e2e/modules/<name>Tests.js`. Cover happy path and error ("sad path") scenarios. Route handlers use `this.post(path, handler, options)` and return `new Response(status, headers, data)`. The example is the StandardSSO widget's AccountInput scenario.

```ts
// server/index.ts, inside routes()
this.post('/accountInputInlineScenario/StepExecutionResult', () => {
  const renderResponse: IRenderResponse = { state: 'Render', renderResult: { /* see Step 4 */ } };
  return new Response(200, {}, renderResponse);
}, { timing: 1000 });
```

`IRenderResponse` and `IRenderResult` are defined under `Scripts/interfaces/` (`state: 'Render'`, `renderResult: { displayMethod: string; formMethod: 'GET' | 'POST'; formFields: { [key: string]: string }; uri: string }`).

```js
// tests/e2e/specs/accountInputScenario.js
import accountInputScenario from '../modules/accountInputTests';

describe('account inline selection scenario', () => {
  describe('desktop', () => {
    beforeEach(() => {
      cy.visit('/accountInputInlineScenario');
      cy.viewport(1000, 1000);
    });

    accountInputScenario();
  });

  describe('mobile', () => {
    beforeEach(() => {
      cy.visit('/mobile/accountInputInlineScenario');
      cy.viewport(375, 812);
    });

    accountInputScenario();
  });
});
```

```js
// tests/e2e/modules/accountInputTests.js
export default () => {
  it('prompts the user to choose an account', () => {
    cy.get('#iris_dropdown_account').click();
    cy.get('.iris-dropdown__display-list').first('.iris-option').click();
    cy.get('.header__actions .iris-button').click();
    cy.get('#_iframe').should('exist');
  });

  it('allows a user to go back and choose a different account', () => {
    // ... same steps, then click the back button and assert the dropdown was cleared
    cy.get('.render__navigation .iris-button').click();
    cy.get('#iris_dropdown_account .iris-dropdown__placeholder')
      .should('contain', 'Select an account');
  });
};
```

Sources: 3. Easy Testing with Cypress (https://confluence.alkami.com/spaces/SDKC/pages/303531395)

### Step 4: mock API requests and responses

Keep `server/index.ts` small by moving responses into `server/mocks/<scenario>.ts`, shared entities into `server/mocks/entities/`, and re-exporting through `server/mocks/index.ts` (`export * from './accountScenario';`).

```ts
// server/mocks/entities/accounts.ts
import { IAccount } from '../../../Scripts/interfaces/IAccount';

export const mockAccounts: IAccount[] = [
  {
    accountNumber: '**8160',
    accountIdentifier: '132342ec-2680-52da-923f-daa13f7ba9af',
    displayName: 'Rewards Checking Account',
    themeColorIndex: 6,
  },
  // ...
];
```

```ts
// server/mocks/accountScenario.ts
import { IRenderResponse } from '../../Scripts/interfaces/IRenderResponse';
import { IInputResponse } from '../../Scripts/interfaces/IInputResponse';
import { mockAccounts } from './entities/accounts';

export const accountInputResponse: IInputResponse = {
  state: 'Input',
  datasets: {
    accounts: mockAccounts,
  },
};

export const accountRenderInlineResponse: IRenderResponse = {
  state: 'Render',
  renderResult: {
    displayMethod: 'Inline',
    formMethod: 'POST',
    formFields: { SAMLResponse: 'blahblahblah' },
    uri: '/testurl/iframepost',
  },
};
```

**Multi-stage responses.** When the same endpoint must return different responses as the scenario progresses (first prompt for input, then render), use a resetting counter:

```ts
// server/index.ts
import { Server, Response } from 'miragejs';
import * as MOCKS from './mocks';

// responseFlags keep track of where you are in the scenario
const responseFlags = {
  accountInputInlineScenario: 0
};

// ... inside routes(), after this.namespace = '/StandardSSO';
this.post('/accountInputInlineScenario/StepExecutionResult', () => {
  // If the flag is 0, prompt for input and flip the flag to 1
  // If it's 1, return the render response
  if (responseFlags.accountInputInlineScenario === 0) {
    responseFlags.accountInputInlineScenario += 1;
    return new Response(200, {}, MOCKS.accountInputResponse);
  }

  responseFlags.accountInputInlineScenario = 0;
  return new Response(200, {}, MOCKS.accountRenderInlineResponse);
}, { timing: 1000 });
```

Sources: 4. Mock API Requests and Responses (https://confluence.alkami.com/spaces/SDKC/pages/303531396)

### E2E best practices: selectors and page objects

Follow the Cypress best practices (https://docs.cypress.io/guides/references/best-practices) and Core Concepts (https://docs.cypress.io/guides/core-concepts/introduction-to-cypress). Test-writing is shared between test engineers and developers, so maintainability is the priority.

**Selectors.** Do not select by class, id, or any attribute that HTML actually uses. Select by the `data-cy` attribute (or text). Apply `data-cy` only during test runs through a directive that checks `process.env.ALBUS_LOCAL_DEV`:

```ts
// testSelectorDirective.ts
export default (el: any, binding: any) => {
  if (process.env.ALBUS_LOCAL_DEV) {
    el.setAttribute('data-cy', binding.value);
  }
};
```

Register it (the source shows the Vue 2 form `Vue.directive('cy-select', TestTags);` in `Main.ts`; in Vue 3 use `app.directive('cy-select', TestTags)`), then use it in templates and tests:

```html
<iris-quick-action-button
  v-cy-select="'create-savings-goal'"
  class="float-right"
  kind="highEmphasis"/>
```

```js
cy.get('[data-cy=create-savings-goal]')
```

Add a custom command in `tests/e2e/support/commands.js` so tests read `cy.getComponent('create-savings-goal')`:

```js
Cypress.Commands.add('getComponent', (value) => {cy.get(`[data-cy=${value}]`)})
```

With a single selector convention, developers and test engineers can agree up front on the list of `data-cy` values per element. The Cypress Selector Playground highlights every element carrying `data-cy` to verify coverage.

**Page Object Model.** One class per page exposing every user action, assertion, and request intercept; tests interact only with those classes. Group multiple actions on one element into a sub-object and `return this` to allow chaining. Methods should do one clearly named action, never a mix of actions and assertions.

```js
// InitialLandingPage.js
export default class InitialLandingPage {
  static verifyPigLottie() {
    cy.getComponent('savings-piggy').should('be.visible');
  }

  static verifyTitle(expectedText) {
    cy.getComponent('title').should('contain', expectedText);
  }

  static verifyDescription(expectedText) {
    cy.getComponent('description').should('contain', expectedText)
  }

  static CreateGoal = {
    verifyText(text) {
      cy.getComponent('create-goal').should('contain', text);
      return this;
    },
    click() {
      cy.getComponent('create-goal').click();
      return this;
    }
  }
}
```

```js
it('render initial layout and form open', () => {
  InitialLandingPage.verifyPigLottie();
  InitialLandingPage.verifyTitle("Savings goals");
  InitialLandingPage.verifyDescription("Savings goals help you set aside money for the things you want. We can help you stay on track and achieve your goals.");
  InitialLandingPage.CreateGoal.verifyText("Create a savings goal").click();

  AddSavingsGoalFormPage.Form.shouldBeVisible()
});
```

Benefits: maintainability, readability (tests read like acceptance criteria), simpler reviews, and parallel developer/test-engineer work once the page object interface is agreed. Drawback: more code, mitigated by the small number of page objects per widget.

**Training.** A hands-on Cypress workshop repo exists at https://bitbucket.corp.alkami.net/projects/AI/repos/cypressworkshop/browse?at=refs%2Fheads%2Fdevelop (answers on branch `implemented-tests`). Recorded sessions: Day 1 https://drive.google.com/file/d/14hZxN-qWTly_vJFonBXlBc3MSaF3bvPR/view?usp=sharing and Days 2 and 3 https://drive.google.com/file/d/1mX22LqpkY97f0siKnM_NSbZHZuvWsJ_e/view?usp=sharing (topics include page objects, `data-cy` custom commands, scenarios and test modules, Mirage vs `Cypress.intercept`). Slides were attached to the page but are not visible here. Setup prerequisite page: Installing VS Code and configuring Environment Locally (pageId=132709297).

Sources: End To End Testing Best Practices (https://confluence.alkami.com/spaces/SDKC/pages/303531381), Cypress Training (https://confluence.alkami.com/spaces/SDKC/pages/303531379)

## 4. Tips and tricks

### Complex interface for a prop

Vue validates only primitive prop types natively. To type a prop with an interface, declare a `Props` interface and pass it to `defineProps<Props>()`; do not destructure the result or reactivity is lost.

```vue
<script setup lang="ts">
import { defineProps } from 'vue';
import { ITenant } from '../interfaces/ITenant';

interface Props {
  tenant: ITenant;
}

// DO NOT DESTRUCTURE PROPS, they will loose their reactivity
const props = defineProps<Props>();
</script>
```

### Deep CSS selectors

Scoped styles append a `data-v-xxxx` attribute selector to every rule. For a nested selector targeting a child element (for example an `svg` inside a component root), the attribute lands on the child, which has no such attribute, so the rule never matches:

```scss
// What we expect:
.node-record__view-more:hover svg { fill: #333333; }
// What actually gets produced
.node-record__view-more:hover svg[data-v-f3f3eg9] { fill: #333333; }
```

Prefix the nested rule with `::v-deep` so the loader places the attribute on the parent instead:

```scss
.node-record__view-more {
  align-self: center;
  display: flex;

  ::v-deep svg { // ::v-deep tells the loader to leave this selector alone
    fill: #333333;
  }
}
// Produced: .node-record__view-more[data-v-f3f3eg9]:hover svg { fill: #333333; }
```

### Fetching initial data

Fetch page data in the composition API `setup` of `App.vue` (or a lifecycle hook such as `onMounted`) by calling a store action; the action fetches, transforms, sets state, and finally flips a `hasLoadedInitialData` flag that the template uses to swap a loader for content.

```vue
<!-- App.vue -->
<script setup lang="ts">
import { useProjectStore } from './store';

const store = useProjectStore();

store.getInitialData();
</script>
```

```ts
// store.ts
import { defineStore } from 'pinia'
import * as Api from '../api/requests';
import * as Translator from '../utils/translation';
import { IProjectStore } from '../interfaces/IProjectStore';

export const useProjectStore = defineStore({
  state: (): IProjectStore => ({
    hasLoadedInitialData: false,
    // ...
  }),
  actions: {
    populateProjectAggregateData(payload: IProjectAggregate) { this.projectAggregate = payload; },
    populateExecutionAggregateData(payload: IExecutionAggregate) { this.executionAggregate = payload; },
    setHasLoadedInitialData(payload: boolean) { this.hasLoadedInitialData = payload; },
    async getInitialData() {
      const projectReferences = await Api.getProjectReferences();
      const projectExecutions = await Promise.all(projectReferences.map((project) => Api.getProjectExecutions(project.id)));

      const translatorCalls = await Promise.all([
        Translator.translateProjectRefsToAggregate(projectReferences),
        Translator.translateExecutionsToAggregate(projectExecutions),
      ]);

      this.populateProjectAggregateData(translatorCalls[0]);
      this.populateExecutionAggregateData(translatorCalls[1]);

      // After all mutations have been executed, set the loading state to true
      this.setHasLoadedInitialData(true);
    },
  },
});
```

(The source omits the store id in `defineStore({...})`; Pinia requires a unique id as the first argument, as shown in the Data Store section.)

Sources: 5. Tips and Tricks (https://confluence.alkami.com/spaces/SDKC/pages/303531397), Complex Interface for a Prop (https://confluence.alkami.com/spaces/SDKC/pages/303531398), Deep CSS selectors (https://confluence.alkami.com/spaces/SDKC/pages/303531399), Fetching Initial Data (https://confluence.alkami.com/spaces/SDKC/pages/303531400)

## 5. Vue 3 migration guide

High-level path for moving an existing widget from the Albus Vue preset (Vue 2) to the Vue 3 preset. Detailed conversion guidance: https://v3-migration.vuejs.org/migration-build.html.

### Upgrade tooling

```
npm uninstall @alkami/albus-module-cypress @alkami/albus-preset-vue
npx -y shx rm -rf node_modules package-lock.json
npm install @alkami/albus-cli@latest @alkami/albus@latest @alkami/albus-preset-vue3@latest @alkami/orb-layout-mock@latest --save-dev
```

### Update configuration

- `albus.config.js`: rename the `vue` preset property to `vue3`.
- `tsconfig.json`: set `extends` to `../node_modules/@alkami/albus-preset-vue3/configs/tsconfig.json`.
- Replace `vue-shim.d.ts` with:

```ts
declare module '*.vue' {
  import Vue from 'vue';
  export default Vue;
}

// makes Vue Module reference the Migration Build Version
declare module 'vue' {
  import { CompatVue } from '@vue/runtime-dom';
  const Vue: CompatVue;
  export default Vue;
  export * from '@vue/runtime-dom';
  const { configureCompat } = Vue;
  export { configureCompat };
}
```

### First build

```
npx albus b -C Alkami.Widget.Client
```

Fix compile-time errors and warnings first (for example use of filters, which are removed in Vue 3). Once the compiler is clean, either switch the compiler to Vue 3 mode or go straight to updating dependencies.

### Configuring Vue 3 mode

- In `albus.config.js`, under the `vue3` `pluginConfiguration`, set `vueCompatMode: '3'`.
- In `app.ts` add `configureCompat({ MODE: 3 });` (imported from `vue`).

Expect many warnings in the CLI and browser console. Tips:

- Filter the browser console to one warning at a time; negated filters such as `-GLOBAL_MOUNT` help.
- Specific deprecations can be suppressed via compat configuration.
- Warnings may come from dependencies (for example vue-router); check the component or stack trace and fix your own source first.
- With vue-router, `<transition>` and `<keep-alive>` do not work with `<router-view>` until Vue Router is updated.
- `<transition>` class names changed and this is the only change with no runtime warning; search the project for `.*-enter` and `.*-leave` CSS.

Update the entry to the new global mounting API:

```ts
import { configureCompat, createApp } from 'vue';
import App from './App.vue';

void (async () => {
  configureCompat({ MODE: 3 });

  const app = createApp(App);

  if (process.env.NODE_ENV === 'development' && process.env.ALBUS_LOCAL_DEV === 'cypress') {
    const { setup } = await import('./tests/e2e/setup')
    await setup();
  }

  app.mount('#app');
})();
```

Known limitations of the migration build (dependency issues and others): https://v3-migration.vuejs.org/migration-build.html#known-limitations.

### Update dependencies

- **Iris Vue**: not compatible with Vue 3 as of the page date (2023-10-11). Widgets using it must wait to upgrade.
- **Vue Router**: no longer bundled in the preset; the widget owns the dependency. `npm install vue-router@4`, then migrate per https://router.vuejs.org/guide/migration/index.html. Result looks like:

```ts
import { createRouter, createWebHistory } from 'vue-router';

// ONLY the entry point or global views should be imported in this way.
import Dashboard from '../views/Dashboard.vue';

const isMobile = location.pathname.toLowerCase().startsWith('/mobile');

const routes = [
  { path: '/', name: 'dashboard', component: Dashboard },
  { path: '/task/:taskId', name: 'task', component: () => import('../views/Task.vue') },
  { path: '/:pathMatch(.*)*', redirect: '/' },
];

const router = createRouter({
  routes,
  history: createWebHistory((isMobile ? '/Mobile/' : '/') + 'Example/'),
});

export default router;
```

- **Vuex**: also removed from the preset. Pinia (https://pinia.vuejs.org/) is the preferred store; Vuex is still maintained.
  - Keep Vuex: `npm install vuex@4` and follow https://vuex.vuejs.org/guide/migrating-to-4-0-from-3-x.html (code example: https://github.com/vuejs/vue-hackernews-2.0/commit/5bfd4c61ee50f358cd5daebaa584f2c3f91e0205).
  - Move to Pinia: `npm install pinia@latest` and follow https://pinia.vuejs.org/cookbook/migration-vuex.html.

Sources: Vue 3 Migration Guide (https://confluence.alkami.com/spaces/SDKC/pages/303531401)
