import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LandingPageComponent } from './pages/landing/landing-page.component';
import { HomePageComponent } from './pages/home/home-page.component';
import { FriendsPageComponent } from './pages/friends/friends-page.component';
import { ChatPageComponent } from './pages/chat/chat-page.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: LandingPageComponent
  },
  {
    path: 'home',
    canActivate: [AuthGuard],
    component: HomePageComponent
  },
  {
    path: 'friends',
    canActivate: [AuthGuard],
    component: FriendsPageComponent
  },
  {
    path: 'chat/:id',
    canActivate: [AuthGuard],
    component: ChatPageComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'top' })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
