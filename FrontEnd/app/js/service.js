function docServiceFunction() {
    this.tocClassApi =  function(navItem) {
        return {
          active: navItem.href && this.currentPage,
          current: this.currentPage === navItem.href,
          'nav-index-section': navItem.type === 'section'
        };
    };
}