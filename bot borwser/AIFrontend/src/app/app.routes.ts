import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', loadComponent: () => import('./components/full-display-ai-chat/full-display-ai-chat.component').then(m => m.FullDisplayAiChatComponent) },
    // { path: '', loadComponent: () => import('./components/card-list/card-list.component').then(m => m.CardListComponent) },
    { path: 'detail/:id', loadComponent: () => import('./components/detail-page/detail-page.component').then(m => m.DetailPageComponent) },
    { path: 'chat/ai', loadComponent: () => import('./components/full-display-ai-chat/full-display-ai-chat.component').then(m => m.FullDisplayAiChatComponent) },

];
