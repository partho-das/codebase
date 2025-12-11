import { Component, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import {CommonModule, NgIf} from '@angular/common';

@Component({
  selector: 'app-scroll-list',
  templateUrl: './scroll-list.component.html',
  standalone: true,
  imports: [
    CommonModule
  ]
})
export class ScrollListComponent implements AfterViewInit {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  items: { id: number; title: string }[] = [];
  loading = false;
  page = 0; // for pagination
  pageSize = 20;

  ngAfterViewInit() {
    this.loadMore(); // initial load
  }

  onScroll() {
    const container = this.scrollContainer.nativeElement;
    const threshold = 50; // when 50px from bottom

    if (!this.loading && container.scrollTop + container.clientHeight >= container.scrollHeight - threshold) {
      this.loadMore();
    }
  }

  loadMore() {
    this.loading = true;

    // Simulate API call
    setTimeout(() => {
      const newItems = Array.from({ length: this.pageSize }).map((_, i) => ({
        id: this.page * this.pageSize + i + 1,
        title: `Item #${this.page * this.pageSize + i + 1}`,
      }));

      this.items = [...this.items, ...newItems];
      this.page++;
      this.loading = false;
    }, 800);
  }

  openItem(item: { id: number; title: string }) {
    // Navigate or open another component
    console.log('Open item', item);
    // e.g. this.router.navigate(['/item', item.id]);
  }
}
