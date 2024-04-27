import { createApp } from 'vue';
import PrimeVue from 'primevue/config';
import Wind from './presets/wind'; 

import App from './App.vue';

import './assets/main.css';

const app = createApp(App);

app.use(PrimeVue, {
    unstyled: true,
    pt: Wind
});

app.mount('#app');
