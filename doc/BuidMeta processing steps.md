Doc-as-Code: BuildMeta.exe Processing Design Specification
==========================================

0. Introduction
---------------
1. Steps
---------------
### 1.1. Step.1. Generate metadata file for each project
### 1.2. Step.2. Merge metadata files from each project to generate metadata.yml
#### 1.2.1 Workflow
`Load Metadata List` => `Merge` => `Export`
##### 1.2.1.1. Merge
1. Merge Namespace
2. Overwrite Classes/Methods with same UID

### 1.3. Step.3. Build Index
`Load API Index`, `Load Markdown File List` => `Run Indexer Pipeline`
#### 1.3.1 Indexer Pipeline
1. *YAML Header* Indexer => `<id>.yml.map` new `key`
2. *Code Snippet* Parser => `<id>.yml.map`'s `key`'s `references` section
                         => `<name>.md.map`'s `references` section
3. *Api Reference* Parser => `<id>.yml.map`'s `key`'s `references` section
                         => `<name>.md.map`'s `references` section
4. *Search* Full-Text Indexer => `index.js`?
