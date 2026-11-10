import { test, expect } from '@playwright/test';

test.use({ channel: 'chrome' });
const origin = 'http://127.0.0.1:4305';
const long = 'IdentificadorOriginalSinEspacios'.repeat(12);
test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => localStorage.setItem('glld_session', JSON.stringify({ accessToken: 'layout-test-only', expiresAt: '2099-01-01T00:00:00Z' })));
  await page.route('**/*', async route => {
    const url = new URL(route.request().url());
    if (url.origin !== origin) return route.abort();
    if (!url.pathname.startsWith('/api/')) return route.continue();
    let data: unknown = { items: [], totalItems: 0, pageNumber: 1, pageSize: 20, totalPages: 1 };
    if (url.pathname === '/api/auth/me') data = { name:'Prueba visual',username:'layout',initials:'PV',role:'ADMINISTRADOR',roleName:'Administrador',centerIds:[],canViewAllCenters:true,permissions:['carga_masiva','inventario','montajes'].map(m=>`modulos.${m}.consultar`) };
    else if (url.pathname.includes('previsualizar')) data = {id:'preview',token:'mock',total:1,validas:0,conError:1,errores:[{fila:2,campo:'Referencia',error:long}],filas:[Object.fromEntries(Array.from({length:18},(_,i)=>['Columna'+i,long]))]};
    else if (url.pathname.includes('metricas')) data = {disponibles:0,enReparacion:0,enReencauche:0,enTraslado:0,bloqueadas:0,conAtencion:0};
    else if (/reservas|solicitudes/.test(url.pathname)) data = [];
    return route.fulfill({json:data});
  });
});

for (const width of [320,390,768,1366,1920]) {
  test(`contiene formulario y tablas a ${width}px, menú abierto/cerrado`, async ({page}, info) => {
    await page.setViewportSize({width,height:900});
    await page.goto(origin+'/carga-masiva');
    await expect(page.getByRole('combobox',{name:'Modalidad',exact:true})).toBeVisible();
    const form = page.locator('.import-form');
    const type = await page.locator('#import-type').boundingBox();
    const mode = await page.locator('#import-mode').boundingBox();
    expect(type).not.toBeNull(); expect(mode).not.toBeNull();
    if(width>650) expect(Math.abs(type!.y-mode!.y)).toBeLessThan(1);
    else expect(mode!.y).toBeGreaterThan(type!.y+type!.height);
    const excel = await page.getByRole('button',{name:'Plantilla Excel',exact:true}).boundingBox();
    expect(excel!.width).toBeLessThan((await form.boundingBox())!.width-25);
    await page.screenshot({path:info.outputPath('formulario.png'),fullPage:true});
    await page.getByLabel('Archivo',{exact:true}).setInputFiles({name:'inventario.csv',mimeType:'text/csv',buffer:Buffer.from('IdentificadorOriginal\n0001')});
    await page.getByRole('button',{name:'Previsualizar',exact:true}).click();
    await expect(page.locator('.preview')).toBeVisible();
    await expect(page.getByRole('button',{name:'Confirmar lote completo'})).toBeDisabled();
    for (const toggled of [false,true]) {
      if(toggled) { await page.locator('.topbar .menu').click(); await page.waitForTimeout(300); }
      const sizes=await page.evaluate(()=>{
        const root=document.querySelector('app-bulk-import')!;
        const main=root.querySelector('main')!;const s=getComputedStyle(main);
        return {width:innerWidth,scroll:document.documentElement.scrollWidth,rootWidth:root.clientWidth,rootScroll:root.scrollWidth,left:main.getBoundingClientRect().left,right:main.getBoundingClientRect().right,paddingLeft:parseFloat(s.paddingLeft),paddingRight:parseFloat(s.paddingRight),bottom:parseFloat(s.paddingBottom)};
      });
      expect(sizes.scroll).toBeLessThanOrEqual(width);
      expect(sizes.rootScroll).toBeLessThanOrEqual(sizes.rootWidth);
      expect(sizes.right).toBeLessThanOrEqual(width+1);
      expect(sizes.paddingLeft).toBe(sizes.paddingRight);
      expect(sizes.bottom).toBeGreaterThanOrEqual(32);
      expect(await page.locator('.preview').evaluate(el=>el.scrollWidth>el.clientWidth)).toBeTruthy();
      await page.screenshot({path:info.outputPath(toggled?'menu-alternado.png':'previsualizacion.png'),fullPage:true});
    }
  });
}

test('compara contenedores de Carga masiva, Inventario y Operaciones',async({page},info)=>{
  await page.setViewportSize({width:1366,height:900});
  const metrics:Record<string,unknown>={};
  for(const [route,selector] of [['carga-masiva','.bulk'],['inventario','.inventory-page'],['montajes','.ops']]){
    await page.goto(origin+'/'+route);await expect(page.locator(selector)).toBeVisible();
    metrics[route]=await page.locator(selector).evaluate(el=>{const r=el.getBoundingClientRect(),s=getComputedStyle(el);return{left:r.left,right:r.right,paddingLeft:s.paddingLeft,paddingRight:s.paddingRight};});
    await page.screenshot({path:info.outputPath(route+'.png'),fullPage:true});
  }
  const m=metrics as Record<string,{left:number;right:number;paddingLeft:string;paddingRight:string}>;
  expect(m['carga-masiva']).toEqual(m['montajes']);
  expect(m['carga-masiva'].left).toBe(m['inventario'].left);
  expect(m['carga-masiva'].right).toBe(m['inventario'].right);
  await info.attach('comparacion',{body:JSON.stringify(metrics,null,2),contentType:'application/json'});
});
