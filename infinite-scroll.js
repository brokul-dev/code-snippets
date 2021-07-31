var infiniteScrollConfig = {
    /* selectors */
    articlesContainerId: 'br-articles-container',
    articleSelector: '#br-articles-container article',
    nextPageLinkSelector: '.nav-previous > a',
    navigationSelector: '.br-common-navigation',

    /* custom classes */
    loadMoreContainerClass: 'br-load-more',
    buttonClass: 'br-load-more__button',    
    loaderClass: 'br-load-more__loader',
    errorInfoClass: 'br-load-more__error',

    /* texts */
    buttonText: 'Load more',
    errorInfoText: 'Something went wrong! Please reload the page.',
};

(function(document, config) {
    function LoadMoreContainer(handleLoadMore) {        
        var element = document.createElement('div');
        element.classList.add(config.loadMoreContainerClass);

        var button = new Button(handleLoadMore);
        element.appendChild(button.htmlElement);

        this.setLoading = function() {
            element.innerHTML = '';
            var loader = new Loader();
            element.appendChild(loader.htmlElement);
        }

        this.setError = function() {
            element.innerHTML = '';
            var errorInfo = new ErrorInfo();
            element.appendChild(errorInfo.htmlElement);
        }

        this.remove = function() {
            element.parentElement.removeChild(element);
        }

        this.htmlElement = element;

        function Button(clickHandler) {        
            var btn = document.createElement('button');		
            btn.classList.add(config.buttonClass);
            btn.textContent = config.buttonText;
            btn.addEventListener('click', clickHandler, false);
            this.htmlElement = btn;
        }

        function Loader() {        
            var loader = document.createElement('div');
            loader.classList.add(config.loaderClass);
            loader.setAttribute('aria-busy', 'true');
            loader.setAttribute('role', 'alert');
            this.htmlElement = loader;
        }

            
        function ErrorInfo() {        
            var errorSpan = document.createElement('span');
            errorSpan.classList.add(config.errorInfoClass);
            errorSpan.textContent = config.errorInfoText;
            this.htmlElement = errorSpan;
        }
    }

    function getNavigation(htmlElement) {
        if(!htmlElement) {
            return null;
        }

        return htmlElement.querySelector(config.navigationSelector);
    }

    function getNavigationLink(htmlElement) {
        if(!htmlElement) {
            return null;
        }

        var navigationNextLink = htmlElement.querySelector(config.nextPageLinkSelector);

        if(navigationNextLink) {
            return navigationNextLink.getAttribute('href');
        }

        return null;
    }

    var content = document.getElementById(config.articlesContainerId);	
    var navigation = getNavigation(content);
    var nextUrl = getNavigationLink(navigation);

    if(content && navigation && nextUrl) {		
        navigation.parentElement.removeChild(navigation);
        appendLoadMoreContainer();

        function appendLoadMoreContainer() {
            var container = new LoadMoreContainer(handleLoadMore);			
            content.appendChild(container.htmlElement);		

            function handleLoadMore() {
                container.setLoading();

                var request = new XMLHttpRequest();
                request.onreadystatechange = function() {
                    if(this.readyState === XMLHttpRequest.DONE) {
                        if (this.status >= 200 && this.status < 400) {
                            container.remove();

                            var responseHtml = document.createElement('html');
                            responseHtml.innerHTML = this.responseText;

                            var articles = responseHtml.querySelectorAll(config.articleSelector);

                            for (var i = 0; i < articles.length; i++) {
                                content.appendChild(articles[i]);						
                            }

                            nextUrl = getNavigationLink(responseHtml);

                            if(nextUrl) {
                                appendLoadMoreContainer();
                            }
                        } else {
                            container.setError();
                        }
                    }
                };

                request.open('GET', nextUrl, true);
                request.send();
            }
        }
    }
}(document, infiniteScrollConfig));