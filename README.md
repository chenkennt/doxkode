Doc-As-Code
===========
Easily build and publish your API documentation. We currently support C# and VB projects.

Getting started
---------------
As a prerequisite, you will need either VS 2015 or VS 2013 with Roslyn compiler installed.
* Install "docascode.msi" from \\\kenchen2\share\docascode
  * Alternatively if you've cloned our source code can grab the build output from \DocAsCode\Drop\Debug\Installer\docascode.msi
* Create new project
* Select Installed > Templates > Visual C# on the left pane, and scroll down to DocumentationWebsite in the center pane
* Add a source project to References (all your triple slash comments will be imported to the final website)
* Build / view in browser (CTRL + SHIFT + W)
* Click "View Source" for an API to route to the source code in GitHub (your API must be pushed to GitHub)

*Note that if you are just adding to table of contents or authoring in markdown, you can simply refresh the page and the changes should take effect. However, you will need to rebuild if you are adding a new markdown file to the API reference section.*

Adding conceptual content
-------------------------
The components of the project you care about now are as follows:
```
api/
article/
  about.md
  index.md
toc.yaml
```
* api/ is reserved for your API section. *Note that in the future we will add an index.md file so you can customize the root page of the API reference.*
* article/ is where you put all of the self-authored topics.
* toc.yaml is the main table of contents file that generates the navbar you will see on the top. It looks like this:
```
- id  : Home
  href: articles/index.md
- id  : About
  href: articles/about.md
- id  : Api Documentation
  href: api
```
Say I want to add another tutorial section, here's how I would go about it:
* Create another folder under article called tutorial, and some additional markdown files as well as another toc.yaml file like so:
```
api/
article/
  tutorial/
    overview.md
    getting_started.md
    lessons.md
    creating_your_first_website.md
    adding_your_own_content.md
    incorporating_code_snippets.md
    publishing_to_github_pages.md
    toc.yaml
  about.md
  index.md
toc.yaml
```
* The root toc.yaml file will have an additional entry for the tutorial section, and the system will know it's a section because there's a toc.yaml in the tutorial folder.
```
- id  : Home
  href: articles/index.md
- id  : About
  href: articles/about.md
- id  : Tutorial
  href: articles/tutorial
- id  : Api Documentation
  href: api
```
* In the tutorial section's toc.yaml file I have the following:
```
- id  : Overview
  href: overview.md
- id  : Getting Started
  href: getting_started.md
- id  : Lessons
  href: lessons.md
  items:
    - id  : Creating Your First Website
      href: creating_your_first_website.md
    - id  : Adding Your Own Content
      href: adding_your_own_content.md
    - id  : Incorporating Code Snippets
      href: incorporating_code_snippets.md
    - id  : Publishing to GitHub Pages
      href: publishing_to_github_pages.md
```
*Note that we have not implemented the index page of sections yet, we plan to do this by expanding the toc.yaml schema to include an index href.*

That's it, refresh your website and you should see the newly created tutorial section. Anything in the markdown files will be dynamically rendered.

A couple of additional details:
* You can use an external link for href.

Adding markdown to API reference
--------------------------------
To make authoring your API reference section easier we allow you to directly add a markdown section to any of your APIs. Create a markdown file and add the following header:
```
---
id: system.string
---
your markdown goes here.
```
Where "id" is the fully qualified name of your API. 

When you build the project again, we will add the markdown section you just created into the reference content of the API. It will be added to the location right after summary and before declarations.

We recommend that all of your API reference markdown files go in the api/ folder, although technically you can put the markdown file anywhere.

###Linking to another API
Currently for linking to another API you simply add an "@" followed by the fully qualified name of your API. Currently we will need to preprocess this syntax so adding this will require a rebuild.

###Including code samples from source
Todo...