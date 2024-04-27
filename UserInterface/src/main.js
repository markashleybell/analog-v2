import { createApp } from 'vue';
import PrimeVue from 'primevue/config';
import Wind from './presets/wind';

import App from './App.vue';

import './assets/main.css';

import editorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';

self.MonacoEnvironment = {
    getWorker(_, label) {
        return new editorWorker();
    },
};

const app = createApp(App);

app.use(PrimeVue, {
    unstyled: true,
    pt: Wind,
});

app.mount('#app');
