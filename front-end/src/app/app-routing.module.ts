import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LandingPageComponent } from './pages/landing/landing-page.component';
import { FriendsPageComponent } from './pages/friends/friends-page.component';
import { ChatPageComponent } from './pages/chat/chat-page.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: LandingPageComponent,
    canActivate: [AuthGuard],
    data: { allowIfLoggedOut: true }  // permette solo guest
  },
  {
    path: 'friends',
    component: FriendsPageComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'chat/:id',
    component: ChatPageComponent,
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '' }
];


@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'top' })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
