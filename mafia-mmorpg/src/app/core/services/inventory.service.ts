import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InventoryItemDto } from '../../shared/models/inventory.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getInventory(): Observable<InventoryItemDto[]> {
    return this.http.get<InventoryItemDto[]>(`${this.apiUrl}/me/inventory`);
  }

  equipItem(itemId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/me/items/equip`, { itemId });
  }
}
