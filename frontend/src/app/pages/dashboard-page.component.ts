import { Component, OnInit, inject } from '@angular/core';
import { RecordsApiService, RecordItem } from '../services/records-api.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  template: `
    <h1>Dashboard</h1>
    <div class="grid">
      <div class="card"><h3>Llantas</h3><p>{{ tires }}</p></div>
      <div class="card"><h3>Vehículos</h3><p>{{ vehicles }}</p></div>
      <div class="card"><h3>Alertas</h3><p>{{ alerts }}</p></div>
      <div class="card"><h3>Críticas</h3><p>{{ critical }}</p></div>
    </div>
    <section class="card" style="margin-top: 1rem;">
      <h3>Últimos registros</h3>
      <ul>
        @for (record of recent; track record.id) {
          <li>{{ record.type }} - {{ record.plate || record.tireId }} - {{ record.createdAt }}</li>
        }
      </ul>
    </section>
  `
})
export class DashboardPageComponent implements OnInit {
  private readonly api = inject(RecordsApiService);
  recent: RecordItem[] = [];
  tires = 0;
  vehicles = 0;
  alerts = 0;
  critical = 0;

  ngOnInit(): void {
    this.api.getAll().subscribe((records) => {
      this.recent = records.slice(0, 5);
      this.tires = records.filter((r) => r.type === 'tire').length;
      this.vehicles = records.filter((r) => r.type === 'vehicle').length;
      this.alerts = records.filter((r) => !!r.alert).length;
      this.critical = records.filter((r) => r.alert.toLowerCase().includes('crítico')).length;
    });
  }
}
