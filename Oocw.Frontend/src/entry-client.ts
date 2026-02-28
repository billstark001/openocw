/**
 * Client-side entry point.
 * In SSR mode Vite uses this file instead of main.ts so that the app is
 * hydrated rather than freshly mounted.
 */
import { createApp } from './app';

const { app, router } = createApp();

// Wait for the router to be ready before mounting so that async components
// and route guards have resolved.
router.isReady().then(() => {
  app.mount('#app');
});
