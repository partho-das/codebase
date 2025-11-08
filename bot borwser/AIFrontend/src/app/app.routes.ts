import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', loadComponent: () => import('./components/card-list/card-list.component').then(m => m.CardListComponent) },
    { path: 'detail/:id', loadComponent: () => import('./components/detail-page/detail-page.component').then(m => m.DetailPageComponent) }
];
