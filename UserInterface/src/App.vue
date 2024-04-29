<script>
import * as monaco from 'monaco-editor';
import MonacoEditor from 'monaco-editor-vue3';
import Button from 'primevue/button';

export default {
    components: {
        MonacoEditor,
        Button,
    },

    data: () => ({
        query: '',
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

    created() {
        monaco.languages.registerCompletionItemProvider('sql', {
            provideCompletionItems: function (model, position) {
                return {
                    suggestions: [
                        {
                            label: 'SELECT',
                            kind: monaco.languages.CompletionItemKind.Keyword,
                            insertText: 'SELECT',
                        },
                        {
                            label: 'FROM',
                            kind: monaco.languages.CompletionItemKind.Keyword,
                            insertText: 'FROM',
                        },
                        {
                            label: 'WHERE',
                            kind: monaco.languages.CompletionItemKind.Keyword,
                            insertText: 'WHERE',
                        },
                        {
                            label: 'entries',
                            kind: monaco.languages.CompletionItemKind.Constant,
                            insertText: 'entries',
                        },
                    ],
                };
            },
        });
    },

    methods: {
        async test() {
            const rsp = await (await fetch('test')).json();

            alert(rsp.msg);
        },
        runQuery() {
            alert(this.query);
        },
        temp(editor) {
            console.log(editor);
        }
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
            @editorDidMount="temp"
        ></MonacoEditor>
        <Button label="Submit" @click="runQuery()"></Button>
        <Button label="TEST" @click="test()"></Button>
    </div>
</template>

<style></style>
