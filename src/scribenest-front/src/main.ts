import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app/app.component';
import { routes, appProviders } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [provideRouter(routes), ...appProviders],
}).catch((err) => console.error(err));
