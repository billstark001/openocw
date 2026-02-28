/**
 * Server-side entry point.
 * Called once per request by the Node.js SSR server.
 * Returns the rendered HTML string for the current URL.
 */
import { renderToString } from 'vue/server-renderer';
import { createApp } from './app';

export async function render(url: string): Promise<{ html: string }> {
  const { app, router } = createApp();

  // Navigate to the requested URL so that the correct components are rendered.
  await router.push(url);
  await router.isReady();

  const html = await renderToString(app);
  return { html };
}
