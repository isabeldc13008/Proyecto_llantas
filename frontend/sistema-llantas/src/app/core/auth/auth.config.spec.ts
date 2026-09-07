import { AuthConfig, validateAuthConfig } from './auth.config';

describe('Entra runtime configuration',()=>{
 const config:AuthConfig={mode:'Entra',tenantId:'11111111-1111-4111-8111-111111111111',clientId:'22222222-2222-4222-8222-222222222222',apiScope:'api://33333333-3333-4333-8333-333333333333/access_as_user'};
 it('resolves the localhost SPA redirect and retains the API scope',()=>{
  const result=validateAuthConfig(config,'http://localhost:4200');
  expect(result.redirectUri).toBe('http://localhost:4200/acceso');
  expect(result.apiScope).toBe(config.apiScope);
 });
 it('explains placeholders before calling Microsoft',()=>{
  expect(()=>validateAuthConfig({...config,tenantId:'<TENANT_ID>',clientId:'<SPA_CLIENT_ID>',apiScope:'<API_SCOPE>'},'http://localhost:4200')).toThrowError(/public\/auth-config.json.*tenantId.*clientId.*apiScope/);
 });
 it('rejects a redirect on another origin',()=>{
  expect(()=>validateAuthConfig({...config,redirectUri:'https://example.org/acceso'},'http://localhost:4200')).toThrowError(/redirectUri/);
 });
 it('restricts local password login to localhost',()=>{
  expect(()=>validateAuthConfig({mode:'Local'},'https://glld.example.org')).toThrowError(/localhost/);
 });
 it('requires a delegated API scope instead of .default',()=>{
  expect(()=>validateAuthConfig({...config,apiScope:'api://api/.default'},'http://localhost:4200')).toThrowError(/apiScope/);
 });
});
