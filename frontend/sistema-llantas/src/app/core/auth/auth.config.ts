// Public identifiers only. Production hosting replaces public/auth-config.json.
export interface AuthConfig { mode: 'Entra' | 'Local'; tenantId?: string; clientId?: string; apiScope?: string }
