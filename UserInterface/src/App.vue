<script setup>
import * as monaco from 'monaco-editor';
import MonacoEditor from 'monaco-editor-vue3';
import Button from 'primevue/button';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import MultiSelect from 'primevue/multiselect';
import InputText from 'primevue/inputtext';
import { ref } from 'vue';

// const baseUrl = '';
const baseUrl = 'http://localhost:8600/';

const query = ref('');
const folder = ref('G:\\My Drive\\Work\\testlogs\\big');
const files = ref([]);
const selectedFiles = ref([]);
const columns = ref([{ field: 'test', header: 'TEST' }]);
const entries = ref([{ test: 'TEST' }]);

const dataLoading = ref(false);

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
