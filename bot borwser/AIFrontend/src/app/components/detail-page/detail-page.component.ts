import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { DataService, CardData } from '../../services/data.service';
import { AiService } from '../../services/ai.service';

@Component({
  selector: 'app-detail-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './detail-page.component.html'
})
export class DetailPageComponent implements OnInit {
  card?: CardData;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private data: DataService,
    private ai: AiService
  ) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.data.getCardDetails(id).subscribe({
      next: (res) => {
        this.card = res;
        this.loading = false;
        // Automatically call backend AI after page loads
        this.ai.chat(`Opened card ${res.title}`).subscribe();
      },
      error: () => this.loading = false
    });
  }
}
