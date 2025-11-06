import { Routes } from '@angular/router';
import { CardListComponent } from './components/card-list/card-list.component';
import { DetailPageComponent } from './components/detail-page/detail-page.component';

export const routes: Routes = [
    { path: '', component: CardListComponent },
    { path: 'detail/:id', component: DetailPageComponent }
];
