import { createSSRApp } from 'vue';
import App from './App.vue';
import { i18n } from './i18n';
import { router } from './router';

/**
 * Factory function used by both the client entry and the server renderer.
 * Each request gets its own app instance to avoid cross-request state pollution.
 */
export function createApp() {
  const app = createSSRApp(App);

  app.use(router);
  app.use(i18n);

  return { app, router };
}
