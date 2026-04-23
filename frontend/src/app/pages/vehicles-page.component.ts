import { Component, OnInit, inject } from '@angular/core';
import { RecordsApiService, RecordItem } from '../services/records-api.service';

@Component({
  standalone: true,
  selector: 'app-vehicles-page',
  template: `
    <h1>Vehículos</h1>
    <div class="card">
      <table style="width:100%">
        <thead>
          <tr><th>Placa</th><th>Centro</th><th>Estado</th></tr>
        </thead>
        <tbody>
          @for (vehicle of vehicles; track vehicle.id) {
            <tr>
              <td>{{ vehicle.plate }}</td>
              <td>{{ vehicle.center }}</td>
              <td><span class="badge">{{ vehicle.status || 'activo' }}</span></td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `
})
export class VehiclesPageComponent implements OnInit {
  private readonly api = inject(RecordsApiService);
  vehicles: RecordItem[] = [];

  ngOnInit(): void {
    this.api.getAll('vehicle').subscribe((records) => (this.vehicles = records));
  }
}
