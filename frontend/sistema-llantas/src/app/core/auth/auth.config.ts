// Public identifiers only. Production hosting replaces public/auth-config.json.
export interface AuthConfig { mode: 'Entra' | 'Local'; tenantId?: string; clientId?: string; apiScope?: string; redirectUri?: string }

export function validateAuthConfig(config: AuthConfig, origin: string): AuthConfig {
 const guid=/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
 const local=['localhost','127.0.0.1','[::1]'].includes(new URL(origin).hostname);
 if(config.mode==='Local'){
  if(!local)throw new Error('Login local solo disponible en localhost.');
  return config;
 }
 if(config.mode!=='Entra')throw new Error('auth-config.json: mode debe ser Entra o Local.');
 const missing=[!guid.test(config.tenantId??'')?'tenantId (<TENANT_ID>)':'',
  !guid.test(config.clientId??'')?'clientId (<SPA_CLIENT_ID>)':'',
  !config.apiScope||/[<>\s]/.test(config.apiScope)||!/^api:\/\/.+\/.+|^https:\/\/.+\/.+/.test(config.apiScope)||config.apiScope.endsWith('/.default')?'apiScope (<API_SCOPE> delegado)':''].filter(Boolean);
 if(missing.length)throw new Error(`Configura public/auth-config.json: ${missing.join(', ')}.`);
 const redirect=new URL(config.redirectUri||'/acceso',origin);
 if(redirect.origin!==origin||redirect.pathname!=='/acceso'||redirect.search||redirect.hash)
  throw new Error('redirectUri debe coincidir con el origen actual y terminar en /acceso.');
 if(redirect.protocol!=='https:'&&!local)throw new Error('Microsoft requiere HTTPS fuera de localhost.');
 return {...config,tenantId:config.tenantId!.toLowerCase(),redirectUri:redirect.href};
}
