<script setup>
import * as monaco from 'monaco-editor';
import MonacoEditor from 'monaco-editor-vue3';
import Button from 'primevue/button';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import MultiSelect from 'primevue/multiselect';
import { ref } from 'vue';

const monacoOptions = {
    language: 'sql',
    roundedSelection: false,
    scrollBeyondLastLine: false,
    theme: 'vs-dark',
    minimap: {
        enabled: false,
    },
    automaticLayout: true,
};

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

const query = ref('');
const files = ref([{ name: 'test1.log', path: 'path' }]);
const selectedFiles = ref([]);
const columns = ref([{ field: 'test', header: 'TEST' }]);
const entries = ref([{ test: 'TEST' }]);

async function test() {
    const rsp = await (await fetch('test')).json();

    alert(rsp.msg);
}

function runQuery() {
    alert(this.query);
}

function temp(editor) {
    console.log(editor);
}
</script>

<template>
    <div id="content">
        <MultiSelect
            v-model="selectedFiles"
            :options="files"
            optionLabel="name"
            placeholder="Select Files"
            :maxSelectedLabels="10"
            class="w-full md:w-20rem"
        />
        <MonacoEditor
            :options="monacoOptions"
            :height="100"
            v-model:value="query"
            @editorDidMount="temp"
        ></MonacoEditor>
        <Button label="Submit" @click="runQuery()"></Button>
        <Button label="TEST" @click="test()"></Button>
        <DataTable
            :value="entries"
            tableStyle="width: 100%"
            size="small"
            paginator
            :rows="5"
            :rowsPerPageOptions="[5, 10, 20, 50]"
        >
            <Column
                v-for="col of columns"
                :key="col.field"
                :field="col.field"
                :header="col.header"
            ></Column>
        </DataTable>
    </div>
</template>

<style></style>
