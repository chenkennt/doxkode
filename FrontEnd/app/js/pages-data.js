// Meta data used by the AngularJS docs app
angular.module('pagesData', [])
  .value('NG_PAGES', {});
// Order matters
angular.module('itemTypes', [])
  .value('NG_ITEMTYPES', {
    "class": {
      "Property": {
        "name": "Property",
        "description": "Properties",
        "show": false
      },
      "Method": {
        "name": "Method",
        "description": "Methods",
        "show": false
      },
      "Constructor": {
        "name": "Constructor",
        "description": "Constructors",
        "show": false
      },
      "Field": {
        "name": "Field",
        "description": "Fields",
        "show": false
      },
    },
    // [
    //   { "name": "Property", "description": "Property" },
    //   { "name": "Method" , "description": "Method"},
    //   { "name": "Constructor" , "description": "Constructor"},
    //   { "name": "Field" , "description": "Field"},
    // ],
    "namespace": {
      "Class": {
        "name": "Class",
        "description": "Classes",
        "show": false
      },
      "Enum": {
        "name": "Enum",
        "description": "Enums",
        "show": false
      },
      "Delegate": {
        "name": "Delegate",
        "description": "Delegates",
        "show": false
      },
      "Interface": {
        "name": "Interface",
        "description": "Interfaces",
        "show": false
      },
      "Struct": {
        "name": "Struct",
        "description": "Structs",
        "show": false
      },
    },
    // [

    //   { "name": "Class", "description": "Class" },
    //   { "name": "Enum" , "description": "Enum"},
    //   { "name": "Delegate" , "description": "Delegate"},
    //   { "name": "Struct" , "description": "Struct"},
    //   { "name": "Interface", "description": "Interface" },
    // ]
  });