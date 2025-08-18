import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryItemDto, EquipItemRequest, UnequipItemRequest } from '../../shared/models/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getInventory(): Observable<InventoryItemDto[]> {
    return this.http.get<InventoryItemDto[]>(`${this.apiUrl}/me/inventory`);
  }

  equipItem(itemId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/me/items/equip`, { itemId });
  }

  unequipItem(itemId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/me/items/unequip`, { itemId });
  }
}
