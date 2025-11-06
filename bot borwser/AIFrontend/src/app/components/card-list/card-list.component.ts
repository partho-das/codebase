import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DataService, CardData } from '../../services/data.service';

@Component({
  selector: 'app-card-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-list.component.html'
})
export class CardListComponent implements OnInit {
  cards: CardData[] = [];
  loading = true;

  constructor(private data: DataService, private router: Router) {}

  ngOnInit() {
    this.data.getCards().subscribe({
      next: (res) => { this.cards = res; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openCard(card: CardData) {
    this.router.navigate(['/detail', card.id]);
  }
}
