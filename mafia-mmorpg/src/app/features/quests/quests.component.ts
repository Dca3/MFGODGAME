import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-quests',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-4xl mx-auto">
        <h1 class="text-4xl font-bold text-mafia-gold mb-8">Görevler</h1>
        
        <div class="bg-mafia-secondary rounded-lg p-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">Mevcut Görevler</h2>
          
          <div class="text-center text-gray-300">
            <p class="text-xl mb-4">Görev sistemi yakında aktif olacak</p>
            <p>NPC'lerden görev alabilecek, tamamlayarak ödüller kazanabileceksin</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class QuestsComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {
    // TODO: Implement quest loading
  }
}
