// ===== STATE =====
let allRecords = [];
let currentView = 'dashboard';
let deleteConfirmId = null;
let selectedTires = {};

const BRANDS = ['Bridgestone','Michelin','Goodyear','Continental','Pirelli','Dunlop','Hankook','Yokohama','Firestone','General'];
const DIMENSIONS = ['11R22.5','12R22.5','295/80R22.5','275/80R22.5','315/80R22.5','225/70R19.5','245/70R19.5','385/65R22.5'];
const OBSERVATIONS = ['Desgaste irregular','Corte lateral','Abultamiento','Desgaste normal','Separación de banda','Impacto','Agrietamiento','OK'];
const POSITIONS = ['1-DI','1-DD','2-EI','2-ED','2-II','2-ID','3-EI','3-ED','3-II','3-ID','Repuesto'];
const VIEW_RENDERERS = {};
const VIEW_FILES = {
  dashboard: './views/dashboard.html',
  vehicles: './views/vehicles.html',
  inventory: './views/inventory.html',
  inspection: './views/inspection.html',
  mounting: './views/mounting.html',
  movements: './views/movements.html',
  schedule: './views/schedule.html',
  alerts: './views/alerts.html'
};
let loadedView = '';

// ===== HELPERS =====
function byType(t){ return allRecords.filter(r=>r.type===t); }
function showToast(msg, error=false){
  const d=document.createElement('div');d.className='toast'+(error?' error':'');d.textContent=msg;
  document.getElementById('toastContainer').appendChild(d);setTimeout(()=>d.remove(),3000);
}
function genId(){ return 'LL-'+Math.random().toString(36).substr(2,8).toUpperCase(); }
function closeModal(){ document.getElementById('modalContainer').innerHTML=''; }
function getDepthClass(v){ v=parseFloat(v); if(isNaN(v))return'badge-info'; if(v>=6)return'badge-ok'; if(v>=3)return'badge-warn'; return'badge-danger'; }
function getDepthColor(v){ v=parseFloat(v); if(isNaN(v))return'#64748b'; if(v>=6)return'#10b981'; if(v>=3)return'#f59e0b'; return'#ef4444'; }
function depthBar(v){ const max=16,pct=Math.min((parseFloat(v)||0)/max*100,100); return `<span>${v||'-'}</span><div class="depth-bar"><div class="depth-fill" style="width:${pct}%;background:${getDepthColor(v)}"></div></div>`; }

function getAlertForDepths(ext,cen,int){
  const vals=[parseFloat(ext),parseFloat(cen),parseFloat(int)].filter(v=>!isNaN(v));
  if(!vals.length)return '';
  const min=Math.min(...vals);
  if(min<2)return 'CRÍTICO - Reemplazo inmediato';
  if(min<4)return 'Desgaste alto - Programar cambio';
  if(Math.max(...vals)-Math.min(...vals)>2)return 'Desgaste irregular';
  return '';
}

// ===== NAVIGATION =====
document.getElementById('sidebarNav').addEventListener('click', async e=>{
  const btn=e.target.closest('[data-view]');
  if(!btn)return;
  currentView=btn.dataset.view;
  document.querySelectorAll('.sidebar-btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  await loadCurrentView();
});

async function loadCurrentView(){
  const mainContent = document.getElementById('mainContent');
  const viewPath = VIEW_FILES[currentView];
  if(!mainContent || !viewPath) return;

  try{
    mainContent.innerHTML = '<p style="color:var(--text2)">Cargando módulo...</p>'; 
    const response = await fetch(viewPath);
    if(!response.ok) throw new Error('No se pudo cargar la vista');
    mainContent.innerHTML = await response.text();
    loadedView = currentView;
    lucide.createIcons();
    refreshCurrentView();
  }catch(error){
    mainContent.innerHTML = '<div class="card"><p style="color:var(--danger)">Error cargando el módulo seleccionado.</p></div>'; 
    console.error(error);
    showToast('No se pudo cargar la vista seleccionada', true);
  }
}

function refreshCurrentView(){
  if(loadedView !== currentView) return;
  const renderView = VIEW_RENDERERS[currentView];
  if (renderView) renderView();
}

// ===== DASHBOARD =====
function renderDashboard(){
  const tires=byType('tire'), vehicles=byType('vehicle'), inspections=byType('inspection');
  document.getElementById('statTires').textContent=tires.length;
  document.getElementById('statVehicles').textContent=vehicles.length;
  const alerts=inspections.filter(i=>i.alert&&i.alert!=='');
  document.getElementById('statAlerts').textContent=alerts.length;
  const critical=inspections.filter(i=>i.alert&&i.alert.includes('CRÍTICO'));
  document.getElementById('statCritical').textContent=critical.length;
  const recent=inspections.slice(-5).reverse();
  const container=document.getElementById('recentInspections');
  if(!recent.length){container.innerHTML='<p style="color:var(--text2);text-align:center">Sin inspecciones registradas</p>';return;}
  container.innerHTML='<table><thead><tr><th>Fecha</th><th>Placa</th><th>ID Llanta</th><th>Profundidades</th><th>Alerta</th></tr></thead><tbody>'+
    recent.map(i=>`<tr><td>${i.date||'-'}</td><td>${i.plate||'-'}</td><td>${i.tire_id||'-'}</td><td>${depthBar(i.depth_ext)} / ${depthBar(i.depth_center)} / ${depthBar(i.depth_int)}</td><td>${i.alert?`<span class="badge ${i.alert.includes('CRÍTICO')?'badge-danger':i.alert.includes('irregular')?'badge-warn':'badge-info'}">${i.alert}</span>`:'-'}</td></tr>`).join('')+'</tbody></table>';
}

// ===== VEHICLES =====
function renderVehicles(){
  const search=(document.getElementById('vehicleSearch')?.value||'').toLowerCase();
  const vehicles=byType('vehicle').filter(v=>!search||v.plate.toLowerCase().includes(search));
  const tires=byType('tire');
  const tbody=document.getElementById('vehicleTableBody');
  if(!vehicles.length){tbody.innerHTML='<tr><td colspan="6" style="text-align:center;color:var(--text2)">Sin vehículos</td></tr>';return;}
  let deleting=deleteConfirmId;
  tbody.innerHTML=vehicles.map(v=>{
    const assigned=tires.filter(t=>t.vehicle_id===v.plate);
    const hasCritical=assigned.some(t=>t.status==='critico');
    const isActive=v.status==='activo';
    const isDel=deleting===v.__backendId;
    return `<tr><td style="font-weight:600">${v.plate}</td><td>${v.center||'-'}</td><td>${assigned.length}</td><td><span class="badge ${hasCritical?'badge-danger':'badge-ok'}">${hasCritical?'Con alertas':'OK'}</span></td><td><span class="badge ${isActive?'badge-ok':'badge-warn'}">${isActive?'Activo':'Inactivo'}</span></td><td><button class="btn btn-sm btn-secondary" onclick="viewVehicleDetail('${v.__backendId}')" style="margin-right:4px">Ver</button><button class="btn btn-sm ${isActive?'btn-warning':'btn-success'}" onclick="toggleVehicleStatus('${v.__backendId}',${isActive})" style="margin-right:4px">${isActive?'Desactivar':'Activar'}</button>${isDel?`<button class="btn btn-sm btn-danger" onclick="confirmDeleteVehicle('${v.__backendId}')">Eliminar</button>`:''}</td></tr>`;
  }).join('');
}

function openVehicleModal(){
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Nuevo Vehículo</h2>
    <form id="vehicleForm" onsubmit="saveVehicle(event)">
      <div class="grid grid-cols-2 gap-4 mb-4">
        <div><label for="vPlate">Placa *</label><input id="vPlate" required placeholder="Ej: JPO569"></div>
        <div><label for="vCenter">Centro *</label><input id="vCenter" required placeholder="Ej: Bogotá"></div>
        <div><label for="vType">Tipo de Vehículo *</label><select id="vType" required onchange="updateAxlesOptions()"><option value="">Seleccionar</option><option value="tractocamion">Tractocamión</option><option value="semiremolque">Semiremolque</option><option value="recomolque">Recomolque</option><option value="cambion">Camión (Cambión)</option></select></div>
        <div><label for="vAxles">Ejes (3-5) *</label><input id="vAxles" type="number" min="3" max="5" value="3" required></div>
        <div><label for="vSpare">Llantas de Repuesto</label><input id="vSpare" type="number" min="0" max="10" value="2"></div>
      </div>
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="vSaveBtn">Guardar</button></div>
    </form></div></div>`;
  lucide.createIcons();
}

function updateAxlesOptions(){
  const type=document.getElementById('vType').value;
  const axlesInput=document.getElementById('vAxles');
  const axlesMap={tractocamion:3,semiremolque:2,recomolque:3,cambion:4};
  if(type && axlesMap[type]){axlesInput.value=axlesMap[type];}
}

async function saveVehicle(e){
  e.preventDefault();
  const plate=document.getElementById('vPlate').value.toUpperCase().trim();
  const center=document.getElementById('vCenter').value.trim();
  const type=document.getElementById('vType').value;
  const axles=parseInt(document.getElementById('vAxles').value)||3;
  const spare=parseInt(document.getElementById('vSpare').value)||0;
  if(byType('vehicle').some(v=>v.plate===plate)){showToast('Ya existe un vehículo con esa placa',true);return;}
  const btn=document.getElementById('vSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span> Guardando...';
  if(allRecords.length>=999){showToast('Límite de 999 registros alcanzado',true);btn.disabled=false;btn.innerHTML='Guardar';return;}
  const r=await window.dataSdk.create({type:'vehicle',plate,center,status:'activo',tire_id:'',brand:'',ref:'',dimension:'',retread:'',position:'',vehicle_id:'',depth_ext:0,depth_center:0,depth_int:0,observation:'',alert:'',technician:'',date:new Date().toISOString(),destination:'',reason:'',notes:'',scheduled_date:'',priority:'',axles,vehicle_type:type,spare_tires:spare});
  if(r.isOk){showToast('Vehículo registrado');closeModal();}else{showToast('Error al guardar',true);btn.disabled=false;btn.innerHTML='Guardar';}
}

async function toggleVehicleStatus(vehicleId,isActive){
  const vehicle=byType('vehicle').find(v=>v.__backendId===vehicleId);
  if(!vehicle)return;
  const newStatus=isActive?'inactivo':'activo';
  const r=await window.dataSdk.update({
    __backendId:vehicleId,
    type:'vehicle',
    plate:vehicle.plate,
    center:vehicle.center||'',
    status:newStatus,
    tire_id:'',brand:'',ref:'',dimension:'',retread:'',position:'',vehicle_id:'',
    depth_ext:0,depth_center:0,depth_int:0,observation:'',alert:'',technician:'',
    date:vehicle.date||new Date().toISOString(),destination:'',reason:'',notes:'',
    scheduled_date:'',priority:'',axles:vehicle.axles||3
  });
  if(r.isOk){
    showToast(newStatus==='activo'?'Vehículo activado':'Vehículo desactivado');
  }else{
    showToast('Error al cambiar estado',true);
  }
}

async function confirmDeleteVehicle(vehicleId){
  const vehicle=byType('vehicle').find(v=>v.__backendId===vehicleId);
  if(!vehicle)return;
  const r=await window.dataSdk.delete(vehicle);
  if(r.isOk)showToast('Vehículo eliminado');else showToast('Error',true);
  deleteConfirmId=null;
}

function abrirSelectTireModal(vehicleId,posicion){
  const vehicle=byType('vehicle').find(v=>v.__backendId===vehicleId);
  if(!vehicle)return;
  const stockTires=byType('tire').filter(t=>t.status==='stock'&&!t.vehicle_id);
  
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Agregar Llanta a ${vehicle.plate} - Posición ${posicion}</h2>
    <div class="search-box mb-4">
      <i data-lucide="search" style="width:16px;height:16px"></i>
      <input type="text" id="selectTireSearch" placeholder="Buscar por ID, marca o dimensión..." oninput="filterSelectTires()" style="width:100%">
    </div>
    <div id="tiresSelectionList" style="display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:12px;max-height:400px;overflow-y:auto;border:1px solid var(--border);border-radius:8px;padding:12px">
      ${stockTires.length?stockTires.map(t=>`<div style="padding:12px;border:1px solid var(--border);border-radius:8px;background:var(--surface2);cursor:pointer" onclick="montarTireEnPosicion('${vehicleId}','${posicion}','${t.__backendId}')">
        <div style="font-weight:600;color:var(--text);margin-bottom:4px">${t.tire_id}</div>
        <div style="font-size:12px;color:var(--text2);margin-bottom:4px">${t.brand}</div>
        <div style="font-size:11px;color:var(--text2);margin-bottom:8px">${t.dimension}</div>
        <button type="button" class="btn btn-sm btn-success" style="width:100%">Seleccionar</button>
      </div>`).join(''):`<div style="grid-column:1/-1;padding:20px;text-align:center;color:var(--text2);font-size:13px">No hay llantas en stock disponibles</div>`}
    </div>
    <div class="flex justify-end gap-2" style="margin-top:16px"><button class="btn btn-secondary" onclick="closeModal()">Cancelar</button></div>
  </div></div>`;
  lucide.createIcons();
}

function filterSelectTires(){
  const search=(document.getElementById('selectTireSearch')?.value||'').toLowerCase();
  const tiles=document.querySelectorAll('#tiresSelectionList > div');
  tiles.forEach(tile=>{
    const text=tile.textContent.toLowerCase();
    tile.style.display=text.includes(search)?'block':'none';
  });
}

async function montarTireEnPosicion(vehicleId,posicion,tireBackendId){
  const vehicle=byType('vehicle').find(v=>v.__backendId===vehicleId);
  const tire=byType('tire').find(t=>t.__backendId===tireBackendId);
  
  if(!vehicle||!tire)return;
  
  // Actualizar la llanta para montarla
  const r=await window.dataSdk.update({
    __backendId:tireBackendId,
    type:'tire',
    tire_id:tire.tire_id,
    brand:tire.brand||'',
    ref:tire.ref||'',
    dimension:tire.dimension||'',
    retread:tire.retread||'',
    position:posicion,
    center:tire.center||'',
    status:'montada',
    vehicle_id:vehicle.plate,
    plate:vehicle.plate,
    depth_ext:tire.depth_ext||0,
    depth_center:tire.depth_center||0,
    depth_int:tire.depth_int||0,
    observation:tire.observation||'',
    alert:tire.alert||'',
    technician:tire.technician||'',
    date:tire.date||new Date().toISOString(),
    destination:'',
    reason:'',
    notes:'',
    scheduled_date:'',
    priority:'',
    axles:0
  });
  
  if(r.isOk){
    showToast(`Llanta ${tire.tire_id} montada en posición ${posicion}`);
    closeModal();
    viewVehicleDetail(vehicleId);
  }else{
    showToast('Error al montar la llanta',true);
  }
}

async function desmontarDesdeDetalle(vehicleId,posicion,tireBackendId){
  const tire=byType('tire').find(t=>t.__backendId===tireBackendId);
  if(!tire)return;
  
  // Actualizar la llanta para desmontar
  const r=await window.dataSdk.update({
    __backendId:tireBackendId,
    type:'tire',
    tire_id:tire.tire_id,
    brand:tire.brand||'',
    ref:tire.ref||'',
    dimension:tire.dimension||'',
    retread:tire.retread||'',
    position:'',
    center:tire.center||'',
    status:'stock',
    vehicle_id:'',
    plate:'',
    depth_ext:tire.depth_ext||0,
    depth_center:tire.depth_center||0,
    depth_int:tire.depth_int||0,
    observation:tire.observation||'',
    alert:tire.alert||'',
    technician:tire.technician||'',
    date:tire.date||new Date().toISOString(),
    destination:'',
    reason:'',
    notes:'',
    scheduled_date:'',
    priority:'',
    axles:0
  });
  
  if(r.isOk){
    showToast(`Llanta ${tire.tire_id} desmontada`);
    viewVehicleDetail(vehicleId);
  }else{
    showToast('Error al desmontar la llanta',true);
  }
}

function viewVehicleDetail(vehicleId){
  const vehicle=byType('vehicle').find(v=>v.__backendId===vehicleId);
  if(!vehicle){showToast('Vehículo no encontrado',true);return;}
  const axles=vehicle.axles||3;
  const tires=byType('tire').filter(t=>t.vehicle_id===vehicle.plate);
  const stockTires=byType('tire').filter(t=>t.status==='stock'&&!t.vehicle_id);
  const spareTires=tires.filter(t=>t.position==='' || t.position==='Repuesto');
  const mountedTires=tires.filter(t=>t.position && t.position!=='' && t.position!=='Repuesto');
  
  // Definir diagrama según tipo de vehículo
  const typeNames={tractocamion:'Tractocamión',semiremolque:'Semiremolque',recomolque:'Recomolque',cambion:'Camión (Cambión)'};
  const vehicleTypeDisplay=typeNames[vehicle.vehicle_type]||`Vehículo (${axles} ejes)`;
  
  const slotOrder=['1-DI','1-DD','2-EI','2-ED','2-II','2-ID','3-EI','3-ED','3-II','3-ID'];
  const slots=[];
  for(let i=0;i<Math.min(axles*2,slotOrder.length);i++){
    const pos=slotOrder[i];
    const tire=mountedTires.find(t=>t.position===pos);
    slots.push({pos,tire});
  }
  
  const diagramHTML=`<div style="display:grid;grid-template-columns:repeat(2,1fr);gap:12px;margin-bottom:16px">
    ${slots.map((s,i)=>`<div style="padding:12px;border:1px solid var(--border);border-radius:8px;background:var(--surface2);display:flex;flex-direction:column;gap:8px">
      <div style="display:flex;justify-content:space-between;align-items:center">
        <div style="font-weight:600;color:var(--accent);font-size:14px">${s.pos}</div>
        ${s.tire?`<button class="btn btn-sm btn-danger" onclick="desmontarDesdeDetalle('${vehicle.__backendId}','${s.pos}','${s.tire.__backendId}')">Desmontar</button>`:`<button class="btn btn-sm btn-success" onclick="abrirSelectTireModal('${vehicle.__backendId}','${s.pos}')">Agregar</button>`}
      </div>
      ${s.tire?`<div><div style="font-size:12px;font-weight:600;color:var(--text)">${s.tire.tire_id}</div><div style="font-size:11px;color:var(--text2)">${s.tire.brand} - ${s.tire.dimension}</div></div>`:`<div style="font-size:12px;color:var(--text2);font-style:italic;text-align:center;padding:8px">Posición vacía</div>`}
    </div>`).join('')}
  </div>`;
  
  const spareHTML=spareTires.length?`<div style="background:rgba(59,130,246,.1);border:1px solid var(--accent);border-radius:8px;padding:12px">
    <h4 style="font-size:13px;font-weight:600;color:var(--accent);margin:0 0 8px;display:flex;align-items:center;gap:6px">
      <i data-lucide="package" style="width:16px;height:16px"></i> Llantas de Repuesto (${spareTires.length}/${vehicle.spare_tires||0})
    </h4>
    <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:8px">
      ${spareTires.map(t=>`<div style="padding:8px;background:var(--surface2);border-radius:6px;font-size:11px">
        <div style="font-weight:600;color:var(--text)">${t.tire_id}</div>
        <div style="color:var(--text2);margin:2px 0">${t.brand}</div>
        <button class="btn btn-sm btn-warning" onclick="desmontarDesdeDetalle('${vehicle.__backendId}','Repuesto','${t.__backendId}')" style="width:100%;margin-top:4px">Desmontar</button>
      </div>`).join('')}
    </div>
  </div>`:'';
  
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:900px;max-height:90%;overflow-y:auto">
    <div class="flex justify-between items-center mb-4"><div><h2 style="font-size:18px;font-weight:700;margin:0">${vehicle.plate}</h2><p style="font-size:12px;color:var(--text2);margin:4px 0">${vehicleTypeDisplay} • ${axles} ejes • Centro: ${vehicle.center}</p></div><button class="btn btn-sm btn-secondary" onclick="closeModal()">Cerrar</button></div>
    
    <div style="background:var(--surface2);border-radius:8px;padding:12px;margin-bottom:16px;display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px">
      <div><label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px">Estado</label><p style="font-weight:600;margin:4px 0;color:var(--text)"><span class="badge ${vehicle.status==='activo'?'badge-ok':'badge-warn'}">${vehicle.status==='activo'?'Activo':'Inactivo'}</span></p></div>
      <div><label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px">Llantas Montadas</label><p style="font-weight:600;margin:4px 0;color:var(--text)">${mountedTires.length}/${axles*2}</p></div>
      <div><label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px">Llantas Repuesto</label><p style="font-weight:600;margin:4px 0;color:var(--text)">${spareTires.length}/${vehicle.spare_tires||0}</p></div>
      <div><label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px">Total Asignadas</label><p style="font-weight:600;margin:4px 0;color:var(--text)">${tires.length}</p></div>
    </div>
    
    <h3 style="font-size:14px;font-weight:600;margin:16px 0 12px;color:var(--text2)">📍 Posiciones de Llantas</h3>
    ${diagramHTML}
    
    ${spareHTML}
  </div></div>`;
  lucide.createIcons();
}

// ===== INVENTORY =====
function renderInventory(){
  const search=(document.getElementById('invSearch')?.value||'').toLowerCase();
  const statusF=document.getElementById('invFilterStatus')?.value||'';
  const centerF=document.getElementById('invFilterCenter')?.value||'';
  let tires=byType('tire');
  if(search)tires=tires.filter(t=>(t.tire_id||'').toLowerCase().includes(search)||(t.brand||'').toLowerCase().includes(search));
  if(statusF)tires=tires.filter(t=>t.status===statusF);
  if(centerF)tires=tires.filter(t=>t.center===centerF);

  // populate center filter
  const centers=[...new Set(byType('tire').map(t=>t.center).filter(Boolean))];
  const sel=document.getElementById('invFilterCenter');
  if(sel){
    const cur=sel.value;
    sel.innerHTML='<option value="">Todos los centros</option>'+centers.map(c=>`<option value="${c}"${c===cur?' selected':''}>${c}</option>`).join('');
  }

  const tbody=document.getElementById('invTableBody');
  if(!tires.length){tbody.innerHTML='<tr><td colspan="9" style="text-align:center;color:var(--text2)">Sin llantas</td></tr>';return;}
  tbody.innerHTML=tires.map(t=>{
    const statusMap={stock:'badge-info',montada:'badge-ok',reencauche:'badge-warn',descarte:'badge-danger'};
    const deleting=deleteConfirmId===t.__backendId;
    return `<tr><td style="font-weight:600">${t.tire_id}</td><td>${t.brand}</td><td>${t.ref||'-'}</td><td>${t.dimension}</td><td>${t.retread||'No'}</td><td><span class="badge ${statusMap[t.status]||'badge-info'}">${t.status||'stock'}</span></td><td>${t.vehicle_id||'-'}</td><td>${t.center||'-'}</td><td>${deleting?`<div class="inline-confirm"><span style="font-size:11px;color:var(--danger)">¿Eliminar?</span><button class="btn btn-sm btn-danger" onclick="confirmDeleteTire('${t.__backendId}')">Sí</button><button class="btn btn-sm btn-secondary" onclick="cancelDelete()">No</button></div>`:`<button class="btn btn-sm btn-danger" onclick="askDeleteTire('${t.__backendId}')"><i data-lucide="trash-2" style="width:12px;height:12px"></i></button>`}</td></tr>`;
  }).join('');
  lucide.createIcons();
}

function askDeleteTire(id){deleteConfirmId=id;renderInventory();}
function cancelDelete(){deleteConfirmId=null;renderInventory();}
async function confirmDeleteTire(id){
  const rec=allRecords.find(r=>r.__backendId===id);
  if(!rec)return;
  const r=await window.dataSdk.delete(rec);
  if(r.isOk)showToast('Llanta eliminada');else showToast('Error',true);
  deleteConfirmId=null;
}

function openTireModal(){
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Nueva Llanta</h2>
    <form id="tireForm" onsubmit="saveTire(event)">
      <div class="grid grid-cols-2 gap-4 mb-4">
        <div><label for="tId">ID Llanta *</label><input id="tId" required value="${genId()}"></div>
        <div><label for="tBrand">Marca *</label><select id="tBrand" required>${BRANDS.map(b=>`<option>${b}</option>`).join('')}</select></div>
        <div><label for="tRef">Referencia</label><input id="tRef" placeholder="Ej: X Multi D"></div>
        <div><label for="tDim">Dimensión *</label><select id="tDim" required>${DIMENSIONS.map(d=>`<option>${d}</option>`).join('')}</select></div>
        <div><label for="tRetread">Reencauche</label><select id="tRetread"><option value="No">No</option><option value="1">1er Reencauche</option><option value="2">2do Reencauche</option><option value="3">3er Reencauche</option></select></div>
        <div><label for="tCenter">Centro *</label><input id="tCenter" required placeholder="Ej: Bogotá"></div>
      </div>
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="tSaveBtn">Guardar</button></div>
    </form></div></div>`;
  lucide.createIcons();
}

async function saveTire(e){
  e.preventDefault();
  const btn=document.getElementById('tSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span>';
  if(allRecords.length>=999){showToast('Límite de registros alcanzado',true);btn.disabled=false;btn.innerHTML='Guardar';return;}
  const r=await window.dataSdk.create({type:'tire',tire_id:document.getElementById('tId').value.trim(),brand:document.getElementById('tBrand').value,ref:document.getElementById('tRef').value.trim(),dimension:document.getElementById('tDim').value,retread:document.getElementById('tRetread').value,center:document.getElementById('tCenter').value.trim(),status:'stock',vehicle_id:'',position:'',plate:'',depth_ext:0,depth_center:0,depth_int:0,observation:'',alert:'',technician:'',date:new Date().toISOString(),destination:'',reason:'',notes:'',scheduled_date:'',priority:''});
  if(r.isOk){showToast('Llanta registrada');closeModal();}else{showToast('Error',true);btn.disabled=false;btn.innerHTML='Guardar';}
}

function openBulkModal(){
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Carga Masiva de Llantas</h2>
    <p style="font-size:13px;color:var(--text2);margin-bottom:12px">Ingrese la cantidad de llantas a generar con parámetros base:</p>
    <form id="bulkForm" onsubmit="saveBulk(event)">
      <div class="grid grid-cols-2 gap-4 mb-4">
        <div><label for="bCount">Cantidad *</label><input id="bCount" type="number" min="1" max="50" value="5" required></div>
        <div><label for="bBrand">Marca</label><select id="bBrand">${BRANDS.map(b=>`<option>${b}</option>`).join('')}</select></div>
        <div><label for="bDim">Dimensión</label><select id="bDim">${DIMENSIONS.map(d=>`<option>${d}</option>`).join('')}</select></div>
        <div><label for="bCenter">Centro</label><input id="bCenter" value="Principal"></div>
      </div>
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="bSaveBtn">Generar</button></div>
    </form></div></div>`;
  lucide.createIcons();
}

async function saveBulk(e){
  e.preventDefault();
  const count=Math.min(parseInt(document.getElementById('bCount').value)||1,50);
  const brand=document.getElementById('bBrand').value;
  const dim=document.getElementById('bDim').value;
  const center=document.getElementById('bCenter').value.trim();
  const btn=document.getElementById('bSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span> Generando...';
  if(allRecords.length+count>999){showToast('Supera el límite de 999 registros',true);btn.disabled=false;btn.innerHTML='Generar';return;}
  let ok=0;
  for(let i=0;i<count;i++){
    const r=await window.dataSdk.create({type:'tire',tire_id:genId(),brand,ref:'',dimension:dim,retread:'No',center,status:'stock',vehicle_id:'',position:'',plate:'',depth_ext:0,depth_center:0,depth_int:0,observation:'',alert:'',technician:'',date:new Date().toISOString(),destination:'',reason:'',notes:'',scheduled_date:'',priority:'',axles:0});
    if(r.isOk)ok++;
  }
  showToast(`${ok} llantas generadas`);closeModal();
}

// ===== INSPECTION =====
function renderInspections(){
  const search=(document.getElementById('inspSearch')?.value||'').toLowerCase();
  let insps=byType('inspection');
  if(search)insps=insps.filter(i=>(i.plate||'').toLowerCase().includes(search)||(i.tire_id||'').toLowerCase().includes(search));
  const tbody=document.getElementById('inspTableBody');
  if(!insps.length){tbody.innerHTML='<tr><td colspan="11" style="text-align:center;color:var(--text2)">Sin inspecciones</td></tr>';return;}
  tbody.innerHTML=insps.reverse().map(i=>{
    const tire=byType('tire').find(t=>t.tire_id===i.tire_id);
    return `<tr>
    <td>${i.date?new Date(i.date).toLocaleDateString():'-'}</td><td style="font-weight:600">${i.plate||'-'}</td><td>${i.mileage||'-'}</td><td>${i.position||'-'}</td><td><button class="btn btn-sm btn-secondary" onclick="viewTireDetail('${i.tire_id}')">${i.tire_id||'-'}</button></td><td>${i.brand||'-'}</td>
    <td>${depthBar(i.depth_ext)}</td><td>${depthBar(i.depth_center)}</td><td>${depthBar(i.depth_int)}</td>
    <td>${i.observation||'-'}</td><td>${i.alert?`<span class="badge ${i.alert.includes('CRÍTICO')?'badge-danger':'badge-warn'}">${i.alert}</span>`:'-'}</td>
  </tr>`;
  }).join('');
}

function viewTireDetail(tireId){
  const tire=byType('tire').find(t=>t.tire_id===tireId);
  if(!tire){showToast('Llanta no encontrada',true);return;}
  const inspections=byType('inspection').filter(i=>i.tire_id===tireId).reverse();
  const movements=byType('movement').filter(m=>m.tire_id===tireId).reverse();
  
  // Construir historial completo combinando inspecciones y movimientos
  const history=[];
  
  // Agregar todas las inspecciones
  inspections.forEach(i=>{
    history.push({
      date:i.date,
      type:'inspección',
      vehicle:i.plate||'-',
      position:i.position||'-',
      detail:`Prof: ${i.depth_ext}/${i.depth_center}/${i.depth_int} mm`,
      alert:i.alert||'',
      observation:i.observation||''
    });
  });
  
  // Agregar todos los movimientos
  movements.forEach(m=>{
    history.push({
      date:m.date,
      type:'movimiento',
      vehicle:m.plate||'-',
      destination:m.destination||'-',
      detail:m.reason||'Movimiento',
      observation:m.notes||''
    });
  });
  
  // Ordenar por fecha descendente
  history.sort((a,b)=>new Date(b.date||0)-new Date(a.date||0));
  
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:900px;max-height:90%;overflow-y:auto">
    <div class="flex justify-between items-center mb-4"><h2 style="font-size:18px;font-weight:700">Llanta: ${tireId}</h2><button class="btn btn-sm btn-secondary" onclick="closeModal()">Cerrar</button></div>
    
    <div class="grid grid-cols-3 gap-4 mb-6">
      <div style="background:var(--surface2);border:1px solid var(--border);border-radius:8px;padding:12px">
        <label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px;display:block;margin-bottom:4px">Marca / Dimensión</label>
        <p style="font-weight:600;font-size:14px;margin:0;color:var(--text)">${tire.brand||'-'} • ${tire.dimension||'-'}</p>
      </div>
      <div style="background:var(--surface2);border:1px solid var(--border);border-radius:8px;padding:12px">
        <label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px;display:block;margin-bottom:4px">Estado Actual</label>
        <p style="margin:0"><span class="badge ${tire.status==='montada'?'badge-ok':tire.status==='reencauche'?'badge-warn':tire.status==='descarte'?'badge-danger':'badge-info'}">${tire.status||'stock'}</span></p>
      </div>
      <div style="background:var(--surface2);border:1px solid var(--border);border-radius:8px;padding:12px">
        <label style="color:var(--text2);font-size:11px;text-transform:uppercase;letter-spacing:0.5px;display:block;margin-bottom:4px">Vehículo / Posición</label>
        <p style="font-weight:600;font-size:13px;margin:0;color:var(--text)">${tire.vehicle_id||'Sin asignar'} ${tire.position?'• '+tire.position:''}</p>
      </div>
    </div>
    
    <h3 style="font-size:14px;font-weight:600;margin:16px 0 12px;display:flex;align-items:center;gap:8px">
      <i data-lucide="history" style="width:16px;height:16px;color:var(--accent)"></i> Historial Completo (${history.length} registros)
    </h3>
    
    ${history.length?`<div style="border-left:2px solid var(--accent);padding-left:0">
      ${history.map((h,idx)=>`<div style="padding:12px;padding-left:16px;border-bottom:1px solid var(--border);position:relative">
        <div style="position:absolute;left:-8px;width:14px;height:14px;border-radius:50%;background:${h.type==='inspección'?'var(--accent2)':'var(--accent)'};border:2px solid var(--bg);top:16px"></div>
        
        <div style="display:flex;justify-content:space-between;align-items:start;margin-bottom:6px">
          <div style="font-weight:600;color:var(--text);font-size:13px">
            ${h.type==='inspección'?'🔍 Inspección':h.type==='movimiento'?'📦 '+h.detail:'📋 '+h.detail}
          </div>
          <div style="font-size:11px;color:var(--text2)">${h.date?new Date(h.date).toLocaleDateString()+' '+new Date(h.date).toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'}):'---'}</div>
        </div>
        
        <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(120px,1fr));gap:8px;font-size:12px;margin-bottom:6px">
          <div><span style="color:var(--text2)">Vehículo:</span> <span style="color:var(--text)">${h.vehicle}</span></div>
          ${h.position?`<div><span style="color:var(--text2)">Posición:</span> <span style="color:var(--text)">${h.position}</span></div>`:h.destination?`<div><span style="color:var(--text2)">Destino:</span> <span style="color:var(--text)">${h.destination}</span></div>`:h.detail?`<div><span style="color:var(--text2)">Detalle:</span> <span style="color:var(--text)">${h.detail}</span></div>`:''}
        </div>
        
        ${h.observation?`<div style="background:rgba(255,255,255,.02);border-left:2px solid var(--border);padding:8px;border-radius:4px;font-size:11px;color:var(--text2);margin-top:6px"><strong>Nota:</strong> ${h.observation}</div>`:''}
        ${h.alert?`<div style="background:rgba(239,68,68,.1);border-left:2px solid var(--danger);padding:8px;border-radius:4px;font-size:11px;color:var(--danger);margin-top:6px"><strong>⚠️ ${h.alert}</strong></div>`:''}
      </div>`).join('')}
    </div>`:'<p style="color:var(--text2);font-size:13px;text-align:center;padding:20px">Sin historial registrado</p>'}
    
    <h3 style="font-size:14px;font-weight:600;margin:20px 0 12px;display:flex;align-items:center;gap:8px">
      <i data-lucide="file-text" style="width:16px;height:16px;color:var(--accent2)"></i> Inspecciones Técnicas (${inspections.length})
    </h3>
    ${inspections.length?`<div class="overflow-x-auto"><table style="font-size:12px"><thead><tr><th>Fecha</th><th>Vehículo</th><th>Pos</th><th>Prof Ext</th><th>Prof Cen</th><th>Prof Int</th><th>Observación</th><th>Alerta</th></tr></thead><tbody>${inspections.map(i=>`<tr><td>${i.date?new Date(i.date).toLocaleDateString():'-'}</td><td>${i.plate||'-'}</td><td>${i.position||'-'}</td><td>${depthBar(i.depth_ext)}</td><td>${depthBar(i.depth_center)}</td><td>${depthBar(i.depth_int)}</td><td style="font-size:11px;color:var(--text2)">${i.observation||'-'}</td><td>${i.alert?`<span class="badge ${i.alert.includes('CRÍTICO')?'badge-danger':'badge-warn'}">${i.alert}</span>`:'-'}</td></tr>`).join('')}</tbody></table></div>`:'<p style="color:var(--text2);font-size:13px">Sin inspecciones registradas</p>'}
  </div></div>`;
  lucide.createIcons();
}

function openInspectionModal(){
  const vehicles=byType('vehicle');
  if(!vehicles.length){showToast('Registre un vehículo primero',true);return;}
  const search=(document.getElementById('invSearch')?.value||'').toLowerCase();
  const filtered=vehicles.filter(v=>v.plate.toLowerCase().includes(search)||v.center.toLowerCase().includes(search));
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-height:90%;overflow-y:auto">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Nueva Inspección</h2>
    <form id="inspForm" onsubmit="saveInspection(event)">
      <div class="grid grid-cols-2 gap-4 mb-4">
        <div><label for="iPlate">Placa Vehículo *</label>
          <div class="search-box"><i data-lucide="search" style="width:16px;height:16px"></i>
          <select id="iPlate" required onchange="loadVehicleTires()">${filtered.map(v=>`<option value="${v.plate}">${v.plate} - ${v.center}</option>`).join('')}</select>
          </div>
        </div>
        <div><label for="iMileage">Kilometraje</label><input id="iMileage" type="number" min="0" placeholder="km"></div>
        <div><label for="iTech">Técnico</label><input id="iTech" placeholder="Nombre técnico"></div>
        <div><label for="iDate">Fecha</label><input id="iDate" type="date" value="${new Date().toISOString().split('T')[0]}"></div>
      </div>
      <h3 style="font-size:14px;font-weight:600;margin:12px 0 8px;color:var(--text2)">Llantas del Vehículo</h3>
      <div id="inspTiresContainer" style="margin-bottom:16px"></div>
      <div id="incompleteWarning" style="display:none;background:rgba(245,158,11,.1);border:1px solid var(--warning);border-radius:8px;padding:12px;margin-bottom:16px">
        <p style="font-size:13px;color:var(--warning);margin:0 0 8px 0"><strong>⚠️ Inspección Incompleta</strong></p>
        <p style="font-size:12px;color:var(--text2);margin:0 0 8px 0">No todas las llantas serán inspeccionadas. Indique la razón:</p>
        <input id="incompleteReason" placeholder="Ej: Vehículo en taller, condición especial..." style="width:100%;margin-bottom:8px">
        <label style="font-size:12px;display:flex;align-items:center;gap:6px;cursor:pointer;color:var(--text2)">
          <input type="checkbox" id="confirmIncomplete"> Confirmar inspección incompleta
        </label>
      </div>
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="iSaveBtn">Guardar Inspección</button></div>
    </form></div></div>`;
  lucide.createIcons();loadVehicleTires();
}

function loadVehicleTires(){
  const plate=document.getElementById('iPlate').value;
  const tires=byType('tire').filter(t=>t.vehicle_id===plate);
  const container=document.getElementById('inspTiresContainer');
  if(!tires.length){container.innerHTML='<p style="color:var(--text2);font-size:13px">Este vehículo no tiene llantas montadas. Monte llantas primero desde el módulo Montaje.</p>';return;}
  
  const depthOptions=Array.from({length:26},(v,i)=>i).map(i=>`<option value="${i}">${i}</option>`).join('');
  
  container.innerHTML=`<div class="overflow-x-auto"><table><thead><tr><th style="width:40px">✓</th><th>Pos</th><th>ID</th><th>Marca</th><th>Dim</th><th>Prof Ext</th><th>Prof Centro</th><th>Prof Int</th><th>Observación</th><th>Destino</th></tr></thead><tbody>${tires.map((t,i)=>`<tr>
    <td><input type="checkbox" id="ichk${i}" checked style="width:16px;height:16px;cursor:pointer" onchange="checkInspectionComplete()"></td>
    <td style="font-weight:600">${t.position||'-'}</td><td>${t.tire_id}</td><td>${t.brand}</td><td>${t.dimension}</td>
    <td><select id="iext${i}" onchange="checkInspectionComplete()">${depthOptions}</select></td>
    <td><select id="icen${i}" onchange="checkInspectionComplete()">${depthOptions}</select></td>
    <td><select id="iint${i}" onchange="checkInspectionComplete()">${depthOptions}</select></td>
    <td><select id="iobs${i}" onchange="checkInspectionComplete()" style="width:150px">${OBSERVATIONS.map(o=>`<option>${o}</option>`).join('')}</select></td>
    <td><select id="idest${i}"><option value="">Normal</option><option value="reencauche">Reencauche</option><option value="disposicion">Disposición Final</option><option value="numeraria">Numeraria</option></select></td>
    <input type="hidden" id="itid${i}" value="${t.tire_id}"><input type="hidden" id="ipos${i}" value="${t.position||''}"><input type="hidden" id="ibrand${i}" value="${t.brand||''}"><input type="hidden" id="ibackendid${i}" value="${t.__backendId||''}">
  </tr>`).join('')}</tbody></table></div>`;
  container.dataset.count=tires.length;
  checkInspectionComplete();
}

function checkInspectionComplete(){
  const count=parseInt(document.getElementById('inspTiresContainer').dataset.count)||0;
  let checked=0;
  for(let i=0;i<count;i++){
    if(document.getElementById('ichk'+i)?.checked)checked++;
  }
  const warning=document.getElementById('incompleteWarning');
  const allTires=count;
  if(checked<allTires){
    warning.style.display='block';
  }else{
    warning.style.display='none';
    document.getElementById('confirmIncomplete').checked=false;
  }
}

async function saveInspection(e){
  e.preventDefault();
  const plate=document.getElementById('iPlate').value;
  const mileage=parseInt(document.getElementById('iMileage').value)||0;
  const tech=document.getElementById('iTech').value.trim();
  const date=document.getElementById('iDate').value||new Date().toISOString();
  const count=parseInt(document.getElementById('inspTiresContainer').dataset.count)||0;
  
  let checked=0;
  for(let i=0;i<count;i++){
    if(document.getElementById('ichk'+i)?.checked)checked++;
  }
  
  // Validar inspección incompleta
  if(checked<count){
    const confirmed=document.getElementById('confirmIncomplete')?.checked;
    const reason=document.getElementById('incompleteReason')?.value;
    if(!confirmed){
      showToast('Confirma la inspección incompleta',true);
      return;
    }
    if(!reason){
      showToast('Indica la razón de la inspección incompleta',true);
      return;
    }
  }
  
  const btn=document.getElementById('iSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span>';
  let saved=0;
  
  for(let i=0;i<count;i++){
    if(!document.getElementById('ichk'+i)?.checked)continue;
    
    const ext=parseInt(document.getElementById('iext'+i)?.value)||0;
    const cen=parseInt(document.getElementById('icen'+i)?.value)||0;
    const int_=parseInt(document.getElementById('iint'+i)?.value)||0;
    const obs=document.getElementById('iobs'+i)?.value||'';
    const tid=document.getElementById('itid'+i)?.value||'';
    const pos=document.getElementById('ipos'+i)?.value||'';
    const brand=document.getElementById('ibrand'+i)?.value||'';
    const dest=document.getElementById('idest'+i)?.value||'';
    
    // Validar que no haya crecimiento de profundidad
    const existingTire=byType('tire').find(t=>t.tire_id===tid);
    let hasGrowth=false;
    if(existingTire){
      if(ext>existingTire.depth_ext || cen>existingTire.depth_center || int_>existingTire.depth_int){
        hasGrowth=true;
      }
    }
    
    if(hasGrowth){
      showToast(`⚠️ Llanta ${tid}: No puede aumentar profundidad`,true);
      continue;
    }
    
    const alert_=getAlertForDepths(ext,cen,int_);
    
    if(allRecords.length+saved>=999){showToast('Límite de registros alcanzado',true);break;}
    
    const r=await window.dataSdk.create({
      type:'inspection',
      plate,
      tire_id:tid,
      brand,
      ref:'',
      dimension:'',
      retread:'',
      position:pos,
      center:'',
      status:'',
      vehicle_id:plate,
      depth_ext:ext,
      depth_center:cen,
      depth_int:int_,
      observation:obs,
      alert:alert_,
      technician:tech,
      date:new Date(date).toISOString(),
      destination:dest,
      reason:'',
      notes:'',
      scheduled_date:'',
      priority:'',
      mileage:mileage,
      axles:0,
      inspection_reason:document.getElementById('incompleteReason')?.value||'',
      partially_inspected:checked<count
    });
    
    if(r.isOk){
      saved++;
      // Actualizar profundidades en la llanta si es menor
      if(existingTire && (ext<existingTire.depth_ext || cen<existingTire.depth_center || int_<existingTire.depth_int)){
        const newExt=Math.min(ext,existingTire.depth_ext);
        const newCen=Math.min(cen,existingTire.depth_center);
        const newInt=Math.min(int_,existingTire.depth_int);
        await window.dataSdk.update({
          __backendId:existingTire.__backendId,
          type:'tire',
          tire_id:existingTire.tire_id,
          brand:existingTire.brand||'',
          ref:existingTire.ref||'',
          dimension:existingTire.dimension||'',
          retread:existingTire.retread||'',
          position:existingTire.position||'',
          center:existingTire.center||'',
          status:existingTire.status||'stock',
          vehicle_id:existingTire.vehicle_id||'',
          plate:existingTire.plate||'',
          depth_ext:newExt,
          depth_center:newCen,
          depth_int:newInt,
          observation:existingTire.observation||'',
          alert:alert_||'',
          technician:tech,
          date:new Date(date).toISOString(),
          destination:'',
          reason:'',
          notes:'',
          scheduled_date:'',
          priority:'',
          mileage:mileage,
          axles:0,
          inspection_reason:'',
          partially_inspected:false
        });
      }
    }
  }
  
  if(saved)showToast(`${saved} inspección(es) registrada(s)`);
  closeModal();
}

// ===== MOUNTING =====
function renderMounting(){
  const mounts=byType('mount');
  const tbody=document.getElementById('mountTableBody');
  if(!mounts.length){tbody.innerHTML='<tr><td colspan="8" style="text-align:center;color:var(--text2)">Sin registros</td></tr>';return;}
  tbody.innerHTML=mounts.reverse().map(m=>`<tr><td>${m.date?new Date(m.date).toLocaleDateString():'-'}</td><td><span class="badge ${m.status==='montar'?'badge-ok':'badge-warn'}">${m.status==='montar'?'Montaje':'Desmontaje'}</span></td><td>${m.plate||'-'}</td><td>${m.tire_id||'-'}</td><td>${m.position||'-'}</td><td>${m.destination||'-'}</td><td>${m.technician||'-'}</td><td>${m.notes||'-'}</td></tr>`).join('');
}

function openMountModal(tipo){
  const vehicles=byType('vehicle');
  const stockTires=byType('tire').filter(t=>tipo==='montar'?(t.status==='stock'&&!t.vehicle_id):t.status==='montada');
  if(!vehicles.length){showToast('Registre un vehículo primero',true);return;}
  if(!stockTires.length){showToast(tipo==='montar'?'No hay llantas en stock disponibles':'No hay llantas montadas',true);return;}

  if(tipo==='montar'){
    // Modal para montar varias llantas
    window.mountingSelections={};
    document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:700px;max-height:90%;overflow-y:auto">
      <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Montar Llantas</h2>
      <form id="mountForm" onsubmit="saveMultipleMount(event)">
        <div class="grid grid-cols-2 gap-4 mb-4">
          <div><label for="mPlate">Vehículo *</label><select id="mPlate" required onchange="updateMountPositions()">${vehicles.map(v=>`<option value="${v.plate}">${v.plate}</option>`).join('')}</select></div>
          <div><label for="mTech">Técnico</label><input id="mTech" placeholder="Nombre"></div>
        </div>
        <h3 style="font-size:13px;font-weight:600;margin:12px 0 8px;color:var(--text2)">Selecciona llantas y posiciones</h3>
        <div id="mountTiresContainer" style="max-height:400px;overflow-y:auto;border:1px solid var(--border);border-radius:8px;padding:12px;margin-bottom:16px"></div>
        <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="mSaveBtn">Montar Seleccionadas</button></div>
      </form></div></div>`;
    updateMountPositions();
  }else{
    const destOptions='<div><label for="mDest">Destino *</label><select id="mDest" required><option value="stock">Stock</option><option value="reencauche">Reencauche</option><option value="descarte">Descarte</option></select></div>';
    document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
      <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Desmontar Llanta</h2>
      <form id="mountForm" onsubmit="saveMount(event,'${tipo}')">
        <div class="grid grid-cols-2 gap-4 mb-4">
          <div><label for="mPlate">Placa *</label><select id="mPlate" required>${vehicles.map(v=>`<option value="${v.plate}">${v.plate}</option>`).join('')}</select></div>
          <div><label for="mTire">Llanta *</label><select id="mTire" required>${stockTires.map(t=>`<option value="${t.tire_id}">${t.tire_id} - ${t.brand}${t.position?' ('+t.position+')':''}</option>`).join('')}</select></div>
          ${destOptions}
          <div><label for="mTech">Técnico</label><input id="mTech" placeholder="Nombre"></div>
          <div><label for="mNotes">Observaciones</label><input id="mNotes" placeholder="Notas"></div>
        </div>
        <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="mSaveBtn">Desmontar</button></div>
      </form></div></div>`;
  }
  lucide.createIcons();
}

function updateMountPositions(){
  const plate=document.getElementById('mPlate').value;
  const veh=byType('vehicle').find(v=>v.plate===plate);
  const axles=veh?.axles||3;
  const mounted=byType('tire').filter(t=>t.vehicle_id===plate);
  const stockTires=byType('tire').filter(t=>t.status==='stock'&&!t.vehicle_id);
  
  const slotOrder=['1-DI','1-DD','2-EI','2-ED','2-II','2-ID','3-EI','3-ED','3-II','3-ID'];
  const slots=[];
  for(let i=0;i<Math.min(axles*2,slotOrder.length);i++){
    const pos=slotOrder[i];
    const tire=mounted.find(t=>t.position===pos);
    slots.push({pos,tire});
  }
  
  let html='';
  slots.forEach((s,idx)=>{
    const selected=window.mountingSelections[s.pos];
    const occupied=s.tire?`<p style="font-size:11px;color:var(--warning);margin:4px 0 0">▸ Ocupada por: ${s.tire.tire_id}</p>`:' ';
    html+=`<div style="padding:12px;border:1px solid var(--border);border-radius:8px;margin-bottom:8px;background:${selected?'rgba(14,165,233,.08)':'var(--surface2)'}">
      <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px">
        <div style="font-weight:600;color:var(--accent);min-width:60px">${s.pos}</div>
        ${occupied}
      </div>
      <select id="pos_${s.pos}" onchange="selectMountTire('${s.pos}',this.value)" style="width:100%">
        <option value="">-- Sin cambios --</option>
        ${stockTires.map(t=>`<option value="${t.tire_id}">${t.tire_id} - ${t.brand} (${t.dimension})</option>`).join('')}
      </select>
      ${selected?`<div style="font-size:11px;color:var(--accent2);margin-top:4px">✓ ${selected}</div>`:''}
    </div>`;
  });
  
  document.getElementById('mountTiresContainer').innerHTML=html||'<p style="color:var(--text2);text-align:center">Sin posiciones disponibles</p>';
}

function selectMountTire(pos,tireId){
  if(!window.mountingSelections)window.mountingSelections={};
  if(tireId){
    window.mountingSelections[pos]=tireId;
  }else{
    delete window.mountingSelections[pos];
  }
}

async function saveMultipleMount(e){
  e.preventDefault();
  const plate=document.getElementById('mPlate').value;
  const tech=document.getElementById('mTech').value.trim();
  const selections=window.mountingSelections||{};
  const selectedCount=Object.keys(selections).length;
  
  if(selectedCount===0){
    showToast('Selecciona al menos una llanta',true);
    return;
  }
  
  const btn=document.getElementById('mSaveBtn');
  btn.disabled=true;
  btn.innerHTML='<span class="loading-spinner"></span> Montando...';
  
  try{
    let mounted=0;
    
    // Para cada posición seleccionada
    for(const [pos,tireId] of Object.entries(selections)){
      // Verificar si hay una llanta ocupando esa posición
      const existingTire=byType('tire').find(t=>t.vehicle_id===plate && t.position===pos);
      
      // Si existe, desmontar y enviar a stock
      if(existingTire){
        const updateResult=await window.dataSdk.update({
          __backendId:existingTire.__backendId,
          type:existingTire.type,
          tire_id:existingTire.tire_id,
          brand:existingTire.brand||'',
          ref:existingTire.ref||'',
          dimension:existingTire.dimension||'',
          retread:existingTire.retread||'',
          position:'',
          center:existingTire.center||'',
          status:'stock',
          vehicle_id:'',
          plate:'',
          depth_ext:existingTire.depth_ext||0,
          depth_center:existingTire.depth_center||0,
          depth_int:existingTire.depth_int||0,
          observation:existingTire.observation||'',
          alert:existingTire.alert||'',
          technician:existingTire.technician||'',
          date:existingTire.date||new Date().toISOString(),
          destination:'',
          reason:'',
          notes:'',
          scheduled_date:'',
          priority:'',
          axles:0
        });
        
        if(!updateResult.isOk){
          showToast(`Error al desmontar ${existingTire.tire_id}`,true);
          continue;
        }
      }
      
      // Montar la nueva llanta
      const tireToMount=byType('tire').find(t=>t.tire_id===tireId);
      if(!tireToMount){
        showToast(`Llanta ${tireId} no encontrada`,true);
        continue;
      }
      
      const updateResult=await window.dataSdk.update({
        __backendId:tireToMount.__backendId,
        type:tireToMount.type,
        tire_id:tireToMount.tire_id,
        brand:tireToMount.brand||'',
        ref:tireToMount.ref||'',
        dimension:tireToMount.dimension||'',
        retread:tireToMount.retread||'',
        position:pos,
        center:tireToMount.center||'',
        status:'montada',
        vehicle_id:plate,
        plate:plate,
        depth_ext:tireToMount.depth_ext||0,
        depth_center:tireToMount.depth_center||0,
        depth_int:tireToMount.depth_int||0,
        observation:tireToMount.observation||'',
        alert:tireToMount.alert||'',
        technician:tech,
        date:new Date().toISOString(),
        destination:'',
        reason:'',
        notes:'',
        scheduled_date:'',
        priority:'',
        axles:0
      });
      
      if(updateResult.isOk)mounted++;
    }
    
    if(mounted>0){
      showToast(`✅ ${mounted} llanta(s) montada(s) correctamente`);
      closeModal();
    }else{
      showToast('No se pudo montar ninguna llanta',true);
    }
  }catch(err){
    showToast('Error en el proceso de montaje',true);
  }finally{
    btn.disabled=false;
    btn.innerHTML='Montar Seleccionadas';
  }
}

function showTiresDiagram(){
  const plate=document.getElementById('mPlate').value;
  const searchTerm=(document.getElementById('tireSearchInput')?.value||'').toLowerCase();
  const veh=byType('vehicle').find(v=>v.plate===plate);
  const axles=veh?.axles||3;
  const mounted=byType('tire').filter(t=>t.vehicle_id===plate);
  let stockTires=byType('tire').filter(t=>t.status==='stock'&&!t.vehicle_id);
  if(searchTerm){
    stockTires=stockTires.filter(t=>(t.tire_id||'').toLowerCase().includes(searchTerm)||(t.brand||'').toLowerCase().includes(searchTerm)||(t.dimension||'').toLowerCase().includes(searchTerm));
  }
  
  // Create slots based on axles
  const slots=[];
  const slotOrder=['1-DI','1-DD','2-EI','2-ED','2-II','2-ID','3-EI','3-ED','3-II','3-ID'];
  for(let i=0;i<Math.min(axles*2,slotOrder.length);i++){
    const pos=slotOrder[i];
    const tire=mounted.find(t=>t.position===pos);
    slots.push({pos,tire});
  }
  
  if(!window.mountingState)window.mountingState={selectedTire:null,selectedPosition:null};
  
  let html=`<h3 style="font-size:14px;font-weight:600;margin-bottom:12px">Posiciones del Vehículo (${axles} ejes)</h3>
    <div class="vehicle-diagram">`;
  
  slots.forEach((s,i)=>{
    const selected=window.mountingState.selectedPosition===s.pos;
    html+=`<div class="tire-slot ${selected?'selected':''}" onclick="selectPosition('${s.pos}')">
      <div class="tire-circle ${selected?'selected':s.tire?'available':'empty'}">${s.pos}</div>
      <span style="font-size:11px;color:var(--text2);text-align:center;word-break:break-word">${s.tire?s.tire.tire_id:'Vacío'}</span>
    </div>`;
  });
  
  html+=`</div>`;
  
  if(window.mountingState.selectedPosition){
    html+=`<div style="background:rgba(16,185,129,.1);border:1px solid var(--accent2);border-radius:8px;padding:12px;margin-bottom:12px">
      <p style="font-size:13px;color:var(--text);margin:0 0 8px 0">Posición seleccionada: <strong>${window.mountingState.selectedPosition}</strong></p>
      <p style="font-size:12px;color:var(--text2);margin:0">Selecciona una llanta del stock para montarla aquí:</p>
    </div>`;
  }
  
  html+=`<h3 style="font-size:14px;font-weight:600;margin-bottom:12px">Llantas Disponibles (${stockTires.length})</h3>
    <div class="search-box mb-4" style="margin-bottom:12px"><i data-lucide="search" style="width:16px;height:16px"></i><input id="tireSearchInput" type="text" placeholder="Buscar ID, marca, dimensión..." style="width:100%" oninput="showTiresDiagram()"></div>
    <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:8px;max-height:300px;overflow-y:auto">`;
  
  if(stockTires.length===0){
    html+=`<div style="grid-column:1/-1;padding:20px;text-align:center;color:var(--text2);font-size:13px">Sin llantas disponibles que coincidan con la búsqueda</div>`;
  }else{
    stockTires.forEach(t=>{
      const isSelected=window.mountingState.selectedTire===t.tire_id;
      html+=`<div style="padding:12px;border:2px solid ${isSelected?'var(--accent2)':'var(--border)'};border-radius:8px;cursor:pointer;background:${isSelected?'rgba(16,185,129,.1)':'var(--surface2)'};transition:all .2s" onclick="selectTire('${t.tire_id}')">
        <div style="font-size:12px;font-weight:600;color:var(--text)">${t.tire_id}</div>
        <div style="font-size:11px;color:var(--text2);margin:4px 0">${t.brand} - ${t.dimension}</div>
        <button type="button" class="btn btn-sm btn-success" style="width:100%;margin-top:6px;${window.mountingState.selectedPosition&&window.mountingState.selectedTire===t.tire_id?'':'pointer-events:none;opacity:0.5'}" onclick="mountSelectedTire(event)">
          <i data-lucide="arrow-up-circle" style="width:12px;height:12px"></i> Montar aquí
        </button>
      </div>`;
    });
  }
  
  html+=`</div>`;
  document.getElementById('vehicleDiagramContainer').innerHTML=html;
  lucide.createIcons();
}

function selectPosition(pos){
  if(!window.mountingState)window.mountingState={};
  window.mountingState.selectedPosition=pos;
  showTiresDiagram();
}

function selectTire(tireId){
  if(!window.mountingState)window.mountingState={};
  window.mountingState.selectedTire=tireId;
  showTiresDiagram();
}

function mountSelectedTire(e){
  e.preventDefault();
  if(!window.mountingState.selectedTire || !window.mountingState.selectedPosition){
    showToast('Selecciona llanta y posición',true);
    return;
  }
  if(!window.selectedTires)window.selectedTires={};
  window.selectedTires[window.mountingState.selectedPosition]={tire_id:window.mountingState.selectedTire};
  window.mountingState.selectedTire=null;
  showTiresDiagram();
}

// ===== MOVEMENTS =====
function renderMovements(){
  const movs=byType('movement');
  const tbody=document.getElementById('movTableBody');
  if(!movs.length){tbody.innerHTML='<tr><td colspan="8" style="text-align:center;color:var(--text2)">Sin movimientos</td></tr>';return;}
  tbody.innerHTML=movs.reverse().map(m=>`<tr><td>${m.date?new Date(m.date).toLocaleDateString():'-'}</td><td><span class="badge badge-info">${m.reason||'-'}</span></td><td>${m.tire_id||'-'}</td><td>${m.plate||'-'}</td><td><span class="badge badge-ok">${m.destination||'-'}</span></td><td>${m.technician||'-'}</td><td>${m.notes||'-'}</td></tr>`).join('');
}

function openMovementModal(){
  const tires=byType('tire');
  if(!tires.length){showToast('No hay llantas registradas',true);return;}
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:600px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Registrar Movimiento</h2>
    <form id="movForm" onsubmit="saveMovement(event)">
      <div class="grid grid-cols-2 gap-4 mb-4">
        <div><label for="mvTire">Llanta *</label><select id="mvTire" required>${tires.map(t=>`<option value="${t.tire_id}">${t.tire_id} - ${t.brand}${t.vehicle_id?' ('+t.vehicle_id+')':''}</option>`).join('')}</select></div>
        <div><label for="mvType">Tipo *</label><select id="mvType" required><option>Rotación</option><option>Reparación</option><option>Reencauche</option><option>Otro</option></select></div>
        <div><label for="mvDest">Destino *</label><select id="mvDest" required><option value="">Seleccionar destino</option><option value="Stock">Stock</option><option value="Rotación">Rotación</option><option value="Reencauche">Reencauche</option><option value="Descarte">Descarte</option><option value="Reparación">Reparación</option></select></div>
        <div><label for="mvTech">Técnico</label><input id="mvTech"></div>
      </div>
      <div class="mb-4"><label for="mvObs">Observación</label><textarea id="mvObs" rows="2" placeholder="Detalles adicionales..."></textarea></div>
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="mvSaveBtn">Registrar</button></div>
    </form></div></div>`;
  lucide.createIcons();
}

async function saveMovement(e){
  e.preventDefault();
  const tireId=document.getElementById('mvTire').value;
  const tire=byType('tire').find(t=>t.tire_id===tireId);
  const destination=document.getElementById('mvDest').value;
  const btn=document.getElementById('mvSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span>';
  if(allRecords.length>=999){showToast('Límite alcanzado',true);btn.disabled=false;btn.innerHTML='Registrar';return;}
  const r=await window.dataSdk.create({type:'movement',plate:tire?.vehicle_id||'',tire_id:tireId,brand:'',ref:'',dimension:'',retread:'',position:'',center:'',status:'',vehicle_id:tire?.vehicle_id||'',depth_ext:0,depth_center:0,depth_int:0,observation:document.getElementById('mvObs').value,alert:'',technician:document.getElementById('mvTech').value,date:new Date().toISOString(),destination:destination,reason:document.getElementById('mvType').value,notes:'',scheduled_date:'',priority:'',axles:0});
  if(r.isOk)showToast('Movimiento registrado');else showToast('Error',true);
  btn.disabled=false;btn.innerHTML='Registrar';
  closeModal();
}

// ===== SCHEDULE =====
function renderSchedule(){
  const prioF=document.getElementById('schedFilterPriority')?.value||'';
  let scheds=byType('schedule');
  if(prioF)scheds=scheds.filter(s=>s.priority===prioF);
  const tbody=document.getElementById('schedTableBody');
  if(!scheds.length){tbody.innerHTML='<tr><td colspan="7" style="text-align:center;color:var(--text2)">Sin programaciones</td></tr>';return;}
  const deleting=deleteConfirmId;
  tbody.innerHTML=scheds.map(s=>{
    const isPast=s.scheduled_date&&new Date(s.scheduled_date)<new Date();
    const isDel=deleting===s.__backendId;
    return `<tr style="${isPast?'background:rgba(239,68,68,.08)':''}"><td>${s.scheduled_date?new Date(s.scheduled_date).toLocaleDateString():'-'} ${isPast?'<span class="badge badge-danger">Vencida</span>':''}</td><td style="font-weight:600">${s.plate||'-'}</td><td>${s.center||'-'}</td><td><span class="badge ${s.priority==='alta'?'badge-danger':s.priority==='media'?'badge-warn':'badge-info'}">${s.priority||'-'}</span></td><td>${s.technician||'-'}</td><td>${s.notes||'-'}</td><td>${isDel?`<div class="inline-confirm"><button class="btn btn-sm btn-danger" onclick="confirmDeleteSched('${s.__backendId}')">Sí</button><button class="btn btn-sm btn-secondary" onclick="cancelDelete()">No</button></div>`:`<button class="btn btn-sm btn-danger" onclick="askDeleteSched('${s.__backendId}')"><i data-lucide="trash-2" style="width:12px;height:12px"></i></button>`}</td></tr>`;
  }).join('');
  lucide.createIcons();
}
function askDeleteSched(id){deleteConfirmId=id;renderSchedule();}
async function confirmDeleteSched(id){const rec=allRecords.find(r=>r.__backendId===id);if(rec){const r=await window.dataSdk.delete(rec);if(r.isOk)showToast('Eliminado');}deleteConfirmId=null;}

function openScheduleModal(){
  const vehicles=byType('vehicle');
  if(!vehicles.length){showToast('Registre un vehículo primero',true);return;}
  document.getElementById('modalContainer').innerHTML=`<div class="modal-overlay" onclick="if(event.target===this)closeModal()"><div class="modal" style="max-width:700px">
    <h2 style="font-size:18px;font-weight:700;margin-bottom:16px">Programar Inspecciones (Masiva)</h2>
    <form id="schedForm" onsubmit="saveSchedule(event)">
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:16px">
        <div><label for="schedType">Tipo de Programación *</label><select id="schedType" required onchange="updateScheduleView()"><option value="individual">Individual</option><option value="massive">Masiva</option></select></div>
        <div><label for="sDate">Fecha Base *</label><input id="sDate" type="date" value="${new Date().toISOString().split('T')[0]}" required></div>
      </div>
      
      <div id="individualView" style="margin-bottom:16px">
        <div class="grid grid-cols-2 gap-4">
          <div><label for="sPlate">Placa *</label><select id="sPlate" required>${vehicles.map(v=>`<option value="${v.plate}">${v.plate} - ${v.center}</option>`).join('')}</select></div>
          <div><label for="sPrio">Prioridad *</label><select id="sPrio" required><option value="alta">Alta</option><option value="media">Media</option><option value="baja">Baja</option></select></div>
          <div><label for="sTech">Técnico</label><input id="sTech"></div>
          <div><label for="sNotes">Notas</label><input id="sNotes" placeholder="Observaciones..."></div>
        </div>
      </div>
      
      <div id="massiveView" style="display:none">
        <h3 style="font-size:13px;font-weight:600;margin-bottom:8px;color:var(--text2)">Selecciona vehículos a programar</h3>
        <div class="search-box mb-4" style="margin-bottom:12px"><i data-lucide="search" style="width:16px;height:16px"></i><input id="massiveVehicleSearch" type="text" placeholder="Buscar por placa o centro..." style="width:100%" oninput="filterMassiveVehicles()"></div>
        <div id="massiveVehiclesList" style="display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:8px;max-height:300px;overflow-y:auto;border:1px solid var(--border);border-radius:8px;padding:12px;margin-bottom:12px">
          ${vehicles.map(v=>`<label style="display:flex;align-items:center;gap:6px;cursor:pointer;font-size:12px;padding:6px;background:var(--surface2);border-radius:6px" data-plate="${v.plate}" data-center="${v.center}">
            <input type="checkbox" class="massive-vehicle" value="${v.plate}"> ${v.plate} - ${v.center}
          </label>`).join('')}
        </div>
        <div class="grid grid-cols-3 gap-4 mb-4">
          <div><label for="mPrio">Prioridad *</label><select id="mPrio" required><option value="alta">Alta</option><option value="media">Media</option><option value="baja">Baja</option></select></div>
          <div><label for="mTech">Técnico</label><input id="mTech" placeholder="Nombre"></div>
          <div><label for="mIntervalDays">Intervalo (días)</label><input id="mIntervalDays" type="number" value="30" min="1"></div>
        </div>
        <div><label for="mNotes">Notas</label><textarea id="mNotes" rows="2" placeholder="Aplica a todas las programaciones..." style="width:100%"></textarea></div>
      </div>
      
      <div class="flex justify-end gap-2"><button type="button" class="btn btn-secondary" onclick="closeModal()">Cancelar</button><button type="submit" class="btn btn-primary" id="sSaveBtn">Programar</button></div>
    </form></div></div>`;
  lucide.createIcons();
}

function updateScheduleView(){
  const type=document.getElementById('schedType').value;
  document.getElementById('individualView').style.display=type==='individual'?'block':'none';
  document.getElementById('massiveView').style.display=type==='massive'?'block':'none';
}

function filterMassiveVehicles(){
  const search=(document.getElementById('massiveVehicleSearch')?.value||'').toLowerCase();
  const labels=document.querySelectorAll('#massiveVehiclesList label');
  labels.forEach(label=>{
    const plate=label.dataset.plate.toLowerCase();
    const center=label.dataset.center.toLowerCase();
    const matches=plate.includes(search)||center.includes(search);
    label.style.display=matches?'flex':'none';
  });
}

async function saveSchedule(e){
  e.preventDefault();
  const type=document.getElementById('schedType').value;
  const baseDate=new Date(document.getElementById('sDate').value);
  const btn=document.getElementById('sSaveBtn');btn.disabled=true;btn.innerHTML='<span class="loading-spinner"></span>';
  
  let scheduled=0;
  let toSchedule=[];
  
  if(type==='individual'){
    const plate=document.getElementById('sPlate').value;
    const veh=byType('vehicle').find(v=>v.plate===plate);
    toSchedule.push({
      plate,
      center:veh?.center||'',
      priority:document.getElementById('sPrio').value,
      technician:document.getElementById('sTech').value,
      notes:document.getElementById('sNotes').value,
      date:baseDate
    });
  }else{
    const checkboxes=document.querySelectorAll('.massive-vehicle:checked');
    const priority=document.getElementById('mPrio').value;
    const tech=document.getElementById('mTech').value;
    const notes=document.getElementById('mNotes').value;
    const interval=parseInt(document.getElementById('mIntervalDays').value)||30;
    
    if(checkboxes.length===0){
      showToast('Selecciona al menos un vehículo',true);
      btn.disabled=false;
      btn.innerHTML='Programar';
      return;
    }
    
    checkboxes.forEach((chk,idx)=>{
      const plate=chk.value;
      const veh=byType('vehicle').find(v=>v.plate===plate);
      const schedDate=new Date(baseDate);
      schedDate.setDate(schedDate.getDate()+idx*interval);
      toSchedule.push({
        plate,
        center:veh?.center||'',
        priority,
        technician:tech,
        notes,
        date:schedDate
      });
    });
  }
  
  if(allRecords.length+toSchedule.length>999){
    showToast('Supera el límite de 999 registros',true);
    btn.disabled=false;btn.innerHTML='Programar';
    return;
  }
  
  for(const sched of toSchedule){
    const r=await window.dataSdk.create({
      type:'schedule',
      plate:sched.plate,
      tire_id:'',brand:'',ref:'',dimension:'',retread:'',position:'',center:sched.center,
      status:'pendiente',vehicle_id:sched.plate,depth_ext:0,depth_center:0,depth_int:0,
      observation:'',alert:'',technician:sched.technician,date:new Date().toISOString(),
      destination:'',reason:'',notes:sched.notes,scheduled_date:sched.date.toISOString(),
      priority:sched.priority,mileage:0,axles:0,inspection_reason:'',partially_inspected:false
    });
    if(r.isOk)scheduled++;
  }
  
  if(scheduled)showToast(`${scheduled} inspección(es) programada(s)`);else showToast('Error',true);
  closeModal();
}

// ===== ALERTS =====
function renderAlerts(){
  const inspections=byType('inspection').filter(i=>i.alert&&i.alert!=='');
  const tires=byType('tire');
  const schedules=byType('schedule').filter(s=>s.scheduled_date&&new Date(s.scheduled_date)<new Date());
  const container=document.getElementById('alertsContainer');
  const items=[];
  inspections.forEach(i=>items.push({type:i.alert.includes('CRÍTICO')?'danger':'warn',text:`Llanta ${i.tire_id} en ${i.plate}: ${i.alert}`,date:i.date}));
  schedules.forEach(s=>items.push({type:'warn',text:`Inspección vencida para ${s.plate} programada ${new Date(s.scheduled_date).toLocaleDateString()}`,date:s.scheduled_date}));
  // Check unassigned tires
  tires.filter(t=>t.status==='stock'&&!t.vehicle_id).slice(0,5).forEach(t=>items.push({type:'info',text:`Llanta ${t.tire_id} en stock sin asignar`,date:t.date}));

  if(!items.length){container.innerHTML='<p style="color:var(--text2);font-size:13px;text-align:center">Sin alertas activas ✅</p>';return;}
  container.innerHTML=items.map(a=>`<div style="display:flex;align-items:center;gap:12px;padding:12px;border-bottom:1px solid var(--border)">
    <div style="width:10px;height:10px;border-radius:50%;background:${a.type==='danger'?'var(--danger)':a.type==='warn'?'var(--warning)':'var(--accent)'};flex-shrink:0"></div>
    <div style="flex:1;font-size:13px">${a.text}</div>
    <div style="font-size:11px;color:var(--text2)">${a.date?new Date(a.date).toLocaleDateString():''}</div>
  </div>`).join('');
}

VIEW_RENDERERS.dashboard = renderDashboard;
VIEW_RENDERERS.vehicles = renderVehicles;
VIEW_RENDERERS.inventory = renderInventory;
VIEW_RENDERERS.inspection = renderInspections;
VIEW_RENDERERS.mounting = renderMounting;
VIEW_RENDERERS.movements = renderMovements;
VIEW_RENDERERS.schedule = renderSchedule;
VIEW_RENDERERS.alerts = renderAlerts;

// ===== DATA SDK =====
const dataHandler = {
  onDataChanged(data){
    allRecords=data;
    refreshCurrentView();
  }
};

// ===== ELEMENT SDK =====
const defaultConfig = {
  app_title: 'TireControl',
  background_color: '#0f1117',
  surface_color: '#1a1d27',
  text_color: '#e2e8f0',
  accent_color: '#3b82f6',
  accent2_color: '#10b981',
  font_family: 'DM Sans',
  font_size: 14
};

window.elementSdk.init({
  defaultConfig,
  onConfigChange: async(config)=>{
    document.getElementById('appTitle').textContent=config.app_title||defaultConfig.app_title;
    const root=document.documentElement;
    root.style.setProperty('--bg',config.background_color||defaultConfig.background_color);
    root.style.setProperty('--surface',config.surface_color||defaultConfig.surface_color);
    root.style.setProperty('--text',config.text_color||defaultConfig.text_color);
    root.style.setProperty('--accent',config.accent_color||defaultConfig.accent_color);
    root.style.setProperty('--accent2',config.accent2_color||defaultConfig.accent2_color);
    const font=config.font_family||defaultConfig.font_family;
    document.body.style.fontFamily=`${font}, sans-serif`;
    const size=config.font_size||defaultConfig.font_size;
    document.body.style.fontSize=size+'px';
  },
  mapToCapabilities:(config)=>({
    recolorables:[
      {get:()=>config.background_color||defaultConfig.background_color,set:v=>{config.background_color=v;window.elementSdk.setConfig({background_color:v})}},
      {get:()=>config.surface_color||defaultConfig.surface_color,set:v=>{config.surface_color=v;window.elementSdk.setConfig({surface_color:v})}},
      {get:()=>config.text_color||defaultConfig.text_color,set:v=>{config.text_color=v;window.elementSdk.setConfig({text_color:v})}},
      {get:()=>config.accent_color||defaultConfig.accent_color,set:v=>{config.accent_color=v;window.elementSdk.setConfig({accent_color:v})}},
      {get:()=>config.accent2_color||defaultConfig.accent2_color,set:v=>{config.accent2_color=v;window.elementSdk.setConfig({accent2_color:v})}}
    ],
    borderables:[],
    fontEditable:{get:()=>config.font_family||defaultConfig.font_family,set:v=>{config.font_family=v;window.elementSdk.setConfig({font_family:v})}},
    fontSizeable:{get:()=>config.font_size||defaultConfig.font_size,set:v=>{config.font_size=v;window.elementSdk.setConfig({font_size:v})}}
  }),
  mapToEditPanelValues:(config)=>new Map([['app_title',config.app_title||defaultConfig.app_title]])
});

(async()=>{
  const r=await window.dataSdk.init(dataHandler);
  if(!r.isOk)console.error('Data SDK init failed');
  lucide.createIcons();
  await loadCurrentView();
})();
