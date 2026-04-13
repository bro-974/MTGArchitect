import { Routes } from '@angular/router';

export const routes: Routes = [
	{
		path: '',
		loadComponent: () => import('./feature/home/home').then((module) => module.Home)
	},
	{
		path: 'feature/server-status',
		loadComponent: () =>
			import('./feature/server-status/server-status').then((module) => module.ServerStatus)
	}
];
