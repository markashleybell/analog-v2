<script setup>
import * as monaco from 'monaco-editor';
import MonacoEditor from 'monaco-editor-vue3';
import Button from 'primevue/button';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import MultiSelect from 'primevue/multiselect';
import InputText from 'primevue/inputtext';
import { ref } from 'vue';
import { analogSqlLanguageDefinition } from './monaco/analog-sql/analog-sql';

// const baseUrl = '';
const baseUrl = 'http://localhost:8600/';

const query = ref('');
const folder = ref('G:\\My Drive\\Work\\testlogs\\kc');
const files = ref([]);
const selectedFiles = ref([]);
const columns = ref([]);
const entries = ref([]);

const dataLoading = ref(false);

const analogSqlMonacoLanguage = 'analog-sql';

monaco.languages.register({
    id: analogSqlMonacoLanguage,
});

monaco.languages.setMonarchTokensProvider(
    analogSqlMonacoLanguage,
    analogSqlLanguageDefinition
);

/*
This is a mutable global, and isn't very Vue... 

But unfortunately, it has to be this way, because of
the bizarre requirement that the provideCompletionItems
function returns completely new objects every time:

https://github.com/microsoft/monaco-editor/issues/1510

So 
*/
let columnCompletionSuggestions = [];

monaco.languages.registerCompletionItemProvider(analogSqlMonacoLanguage, {
    triggerCharacters: [' '],
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
                ...columnCompletionSuggestions.map((c) => ({
                    label: c,
                    kind: monaco.languages.CompletionItemKind.Constant,
                    insertText: c,
                })),
            ],
        };
    },
});

const monacoOptions = {
    language: analogSqlMonacoLanguage,
    roundedSelection: false,
    scrollBeyondLastLine: false,
    theme: 'vs-dark',
    minimap: {
        enabled: false,
    },
    automaticLayout: true,
};

async function loadFileList() {
    const rsp = await fetch(baseUrl + 'getfiles?folder=' + folder.value);
    const json = await rsp.json();

    files.value = json.files;
}

async function loadData() {
    dataLoading.value = true;

    const rsp = await fetch(baseUrl + 'loaddata', {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(selectedFiles.value.map((f) => f.path)),
    });

    const json = await rsp.json();

    columns.value = json.databaseColumns.map((c) => ({
        field: c.name,
        header: c.name,
    }));

    columnCompletionSuggestions = columns.value.map((c) => c.field);

    console.log(columnCompletionSuggestions);

    dataLoading.value = false;
}

function runQuery() {
    alert(query.value);
}
</script>

<template>
    <div id="content">
        <div class="flex">
            <InputText type="text" v-model="folder" class="w-full md:w-20rem" />
            <Button
                label="Load File List"
                @click="loadFileList()"
                class="w-48"
            ></Button>
        </div>
        <div class="flex">
            <MultiSelect
                v-model="selectedFiles"
                :options="files"
                optionLabel="path"
                placeholder="Select Files"
                :maxSelectedLabels="10"
                class="w-full md:w-20rem"
            />
            <Button
                label="Load Data"
                @click="loadData()"
                :loading="dataLoading"
                class="w-48"
            ></Button>
        </div>
        <MonacoEditor
            :options="monacoOptions"
            :height="100"
            v-model:value="query"
        ></MonacoEditor>
        <Button label="Submit" @click="runQuery()"></Button>
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
