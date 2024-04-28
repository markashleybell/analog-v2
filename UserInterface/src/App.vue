<script>
import MonacoEditor from 'monaco-editor-vue3';
import Button from 'primevue/button';

export default {
    components: {
        MonacoEditor,
        Button,
    },

    data: () => ({
        query: 'SELECT * FROM entries',
        options: {
            language: 'sql',
            roundedSelection: false,
            scrollBeyondLastLine: false,
            theme: 'vs-dark',
            minimap: {
                enabled: false,
            },
            automaticLayout: true,
        },
    }),

    watch: {},

    created() {},

    methods: {
        async test() {
            const rsp = await (await fetch('test')).json();

            alert(rsp.msg);
        },
        runQuery() {
            alert(this.query);
        },
    },
};
</script>

<template>
    <div id="content">
        <MonacoEditor
            :options="options"
            :width="800"
            :height="300"
            v-model:value="query"
        ></MonacoEditor>
        <Button label="Submit" @click="runQuery()"></Button>
        <Button label="TEST" @click="test()"></Button>
    </div>
</template>

<style></style>
