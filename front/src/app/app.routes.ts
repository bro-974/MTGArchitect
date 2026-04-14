import { Routes } from '@angular/router';

export const routes: Routes = [
	{
		path: '',
		loadComponent: () => import('./feature/home/home').then((module) => module.Home)
	},
	{
		path: 'feature/card-explorer',
		loadComponent: () =>
			import('./feature/card-explorer/card-explorer').then((module) => module.CardExplorer)
	},
	{
		path: 'feature/server-status',
		loadComponent: () =>
			import('./feature/server-status/server-status').then((module) => module.ServerStatus)
	}
];
