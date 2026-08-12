export default {
  async fetch(request, env) {
    const response = await env.ASSETS.fetch(request);
    if (response.status !== 404) return response;
    const url = new URL(request.url);
    if (url.pathname.startsWith('/api/')) return new Response(JSON.stringify({ message: 'La API empresarial se conecta de forma privada.' }), { status: 503, headers: { 'content-type': 'application/json' } });
    return env.ASSETS.fetch(new Request(new URL('/index.html', url), request));
  }
};
