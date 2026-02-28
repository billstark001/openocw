


import MainPage from '@/pages/MainPage.vue';
import RegisterPage from '@/pages/RegisterPage.vue';
import InfoPage from '@/pages/InfoPage.vue';
import UserPage from '@/pages/UserPage.vue';
import DBPage from '@/pages/DBPage.vue';
import SearchPage from '@/pages/SearchPage.vue';
import CourseDetailPage from '@/pages/CourseDetailPage.vue';
import NotificationPage from '@/pages/NotificationPage.vue';
import AdminPage from '@/pages/AdminPage.vue';
import ReviewPage from '@/pages/ReviewPage.vue';
import ClassInstancePage from '@/pages/ClassInstancePage.vue';
import AssignmentPage from '@/pages/AssignmentPage.vue';

import { createRouter, createWebHashHistory } from 'vue-router';

const routes = [
  { path: '/', component: MainPage, },
  { path: '/auth', component: RegisterPage, },
  { path: '/info', component: InfoPage, },
  { path: '/db', component: DBPage, },
  { path: '/db/:target', component: DBPage, },
  { path: '/user', component: UserPage, },
  { path: '/search', component: SearchPage },
  { path: '/course/:code', component: CourseDetailPage },
  { path: '/notifications', component: NotificationPage },
  { path: '/admin', component: AdminPage },
  { path: '/review', component: ReviewPage },
  { path: '/class-instance/:id', component: ClassInstancePage },
  { path: '/assignment/:instanceId/:contentId', component: AssignmentPage },
];

export const router = createRouter({
  history: createWebHashHistory(),
  routes: routes,
});

export function shouldBeActive(routeIn: string, target: string, precise?: boolean): boolean {
  return precise ? routeIn === target : routeIn.startsWith(target);
}
