angular.module("umbraco").controller("AkismetController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.loading = true;
    vm.stats = [];

    angular.element(function () {
        init();
    });

    var init = function () {
        $http({
            method: 'POST',
            url: '/Umbraco/backoffice/Api/AkismetApi/VerifyKey',
            cache: false
        }).then(function (data) {
            // data: data, status, headers, config
            vm.loading = false;
            document.getElementById('akismet-status').style.display = null;
            if (data.data) {
                document.getElementById('akismet-status').classList.add("alert", "alert-success");
                document.getElementById('akismet-status').innerText = "API Key Valid!";
                
                vm.loading = true;
                getAccount();

            } else {
                document.getElementById('akismet-status').classList.add("alert", "alert-error");
                document.getElementById('akismet-status').innerText = "API Key not found or not valid - please go to Configuration to update";
            }
        });
    };

    var getAccount = function () {
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetAccount',
            cache: false
        }).then(function (data) {
            // data: data, status, headers, config
            vm.loading = false;
            var acct = document.getElementById('akismet-account');
            
            if (data.status == 200) {
                var ul = document.createElement('ul');
                var li = document.createElement('li');
                li.innerHTML = '<strong>Account Type:</strong> ' + data.data.AccountName;
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>Account Status:</strong> ' + data.data.Status;
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>Limit Reached:</strong> ' + data.data.LimitReached;
                ul.appendChild(li);
                if (new Date(data.data.NextBillingDate).toISOString() != new Date(0).toISOString()) {
                    li = document.createElement('li');
                    li.innerHTML = '<strong>Next Billing Date:</strong> ' + new Date(data.data.NextBillingDate).toDateString();
                    ul.appendChild(li);
                } else {
                    li = document.createElement('li');
                    li.innerHTML = '<strong>Next Billing Date:</strong> <em>N/A</em>';
                    ul.appendChild(li);
                }
                acct.appendChild(ul);
                
                vm.loading = true;
                getStats();
            } else {
                acct.classList.add("alert", "alert-error");
                acct.innerText = "There was a problem loading account details";
            }
        });
    };

    var getStats = function () {
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetStats',
            cache: false
        }).then(function (data) {
            // data: data, status, headers, config
            vm.loading = false;

            var date = new Intl.DateTimeFormat('en-US', { month: "long", year: "numeric" });
            var stats = document.getElementById('akismet-stats');
            var history = document.getElementById('akismet-history');
            console.log(data);
            if (data.status == 200) {
                var ul = document.createElement('ul');
                var li = document.createElement('li');
                li.innerHTML = '<strong>Accuracy:</strong> ' + data.data.Accuracy + ' %';
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>Spam:</strong> ' + data.data.Spam;
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>Ham:</strong> ' + data.data.Ham;
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>Missed Spam:</strong> ' + data.data.MissedSpam;
                ul.appendChild(li);
                li = document.createElement('li');
                li.innerHTML = '<strong>False Positives:</strong> ' + data.data.FalsePositives;
                ul.appendChild(li);
                
                stats.appendChild(ul);

                ul = document.createElement('ul');
                li = document.createElement('li');
                for (const property in data.data.Breakdown) {
                    var monthDate = new Date(data.data.Breakdown[property].Da);
                    var prop = data.data.Breakdown[property];

                    li = document.createElement('li');
                    li.innerHTML = '<strong>' + date.format(monthDate) + '</strong>';

                    var innerUl = document.createElement('ul');
                    var innerLi = document.createElement('li');
                    innerLi.innerHTML = '<strong>Spam:</strong> ' + prop.Spam;
                    innerUl.appendChild(innerLi);
                    innerLi = document.createElement('li');
                    innerLi.innerHTML = '<strong>Ham:</strong> ' + prop.Ham;
                    innerUl.appendChild(innerLi);
                    innerLi = document.createElement('li');
                    innerLi.innerHTML = '<strong>Missed Spam:</strong> ' + prop.MissedSpam;
                    innerUl.appendChild(innerLi);
                    innerLi = document.createElement('li');
                    innerLi.innerHTML = '<strong>False Positives:</strong> ' + prop.FalsePositives;
                    innerUl.appendChild(innerLi);
                    li.appendChild(innerUl);
                    ul.prepend(li);
                }

                history.appendChild(ul);

            } else {
                document.getElementById('akismet-stats').classList.add("alert", "alert-error");
                document.getElementById('akismet-stats').innerText = "There was a problem retrieving Akismet Statistics. Please try again later.";
            }
        });
    };
});

angular.module("umbraco").controller("AkismetConfigController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.page = {
        name: "Configuration"
    };
    //console.log(vm);
    //console.log($scope);

    $scope.name = "API Key";

    var init = function () {
        document.getElementById('akismetUrl').setAttribute("placeholder", 'eg - http://' + window.location.hostname + '/');

        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetConfig',
            cache: false
        }).then(function (data) {
            // data: data, status, headers, config
            //console.log(data);
            if (data.status == 200) {
                document.getElementById('akismetKey').value = data.data.key;
                document.getElementById('akismetUrl').value = data.data.blogUrl;
            } else {
                notificationsService.error("Configuration could not be loaded");
            }
        });
    };

    angular.element(function () {
        init();
    });

    vm.Save = function () {
        vm.buttonState = "busy";
        var key = document.getElementById('akismetKey').value;
        var blogUrl = document.getElementById('akismetUrl').value;
        var valid = true;

        if (key.trim().length != 12) {
            vm.buttonState = "error";
            notificationsService.error("API key invalid");
            valid = false;
        }

        if (blogUrl.trim().length < 1 && valid) {
            vm.buttonState = "error";
            valid = false;
            notificationsService.error("URL invalid - must be your site's home page");
        }

        if (valid) {
            $http({
                method: 'POST',
                url: '/Umbraco/backoffice/Api/AkismetApi/VerifyKey?key=' + key + '&blogUrl=' + blogUrl,
                cache: false
            }).then(function (data) {
                // data: data, status, headers, config

                if (data.data) {
                    vm.buttonState = "success";
                    notificationsService.success("API key saved. Return to the overview to see more information.");
                } else {
                    vm.buttonState = "error";
                    notificationsService.error("API key invalid");
                }
            });
        }
    };

    function extractHostname(url) {
        var hostname;
        //find & remove protocol (http, ftp, etc.) and get hostname

        if (url.indexOf("//") > -1) {
            hostname = url.split('/')[2];
        }
        else {
            hostname = url.split('/')[0];
        }

        //find & remove port number
        hostname = hostname.split(':')[0];
        //find & remove "?"
        hostname = hostname.split('?')[0];

        return hostname;
    }
});

angular.module("umbraco").controller("AkismetCommentsController", function ($scope, $http, notificationsService, listViewHelper) {
    var vm = this;
    vm.page = {
        name: "Comments"
    };
    vm.pagination = {
        pageNumber: 1,
        totalPages: 1
    };
    vm.options = {
        includeProperties: [
            { alias: "date", header: "Date", allowSorting: false },
            { alias: "comment", header: "Comment", allowSorting: false },
            { alias: "ip", header: "User's IP", allowSorting: false }
        ]
    };
    vm.items = [];
    vm.selection = [];
    $scope.options = {
        filter: '',
        orderBy: "date",
        orderDirection: "asc",
        bulkActionsAllowed: true
    };
    vm.deleteState = "";
    var date = new Intl.DateTimeFormat('en-US', { dateStyle: "short", timeStyle: "short" });

    var init = function () {
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetComments',
            cache: false
        }).then(function (data) {
            //console.log(data);
            for (var i = 0; i < data.data.length; i++) {
                var d = data.data[i];
                vm.items.push({
                    "name": d.UserName,
                    "date": date.format(new Date(d.CommentDate)),
                    "comment": d.CommentText,
                    "ip": d.UserIp,
                    "id": d.Id
                })
            }
        });
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetCommentPageCount',
            cache: false
        }).then(function (data) {
            var pages = data.data;
            if (pages == 0)
                pages = 1;
            vm.pagination.totalPages = pages;
        });
    };

    angular.element(function () {
        init();
    });

    vm.selectItem = selectItem;
    vm.clickItem = clickItem;
    vm.selectAll = selectAll;
    vm.isSelectedAll = isSelectedAll;
    vm.isSortDirection = isSortDirection;
    vm.sort = sort;
    vm.changePage = changePage;
    vm.allowSelectAll = true;
    vm.deleteComments = deleteComments;

    function changePage(pageNumber) {
        if (pageNumber != undefined) {
            //console.log(pageNumber);
            vm.items = [];
            $http({
                method: 'GET',
                url: '/Umbraco/backoffice/Api/AkismetApi/GetComments?page=' + pageNumber,
                cache: false
            }).then(function (data) {
                //console.log(data);
                for (var i = 0; i < data.data.length; i++) {
                    var d = data.data[i];
                    vm.items.push({
                        "name": d.UserName,
                        "date": date.format(new Date(d.CommentDate)),
                        "comment": d.CommentText,
                        "ip": d.UserIp,
                        "id": d.Id
                    })
                }
            });
        }
    }
 
    function selectAll($event) {
        listViewHelper.selectAllItemsToggle(vm.items, vm.selection);
        toggleBulkActions();
    }
 
    function isSelectedAll() {
        return listViewHelper.isSelectedAll(vm.items, vm.selection);
    }
 
    function clickItem(item) {
        //alert("click node");
    }
 
    function selectItem(selectedItem, $index, $event) {
        //console.log(vm.selection);
        listViewHelper.selectHandler(selectedItem, $index, vm.items, vm.selection, $event);
        toggleBulkActions();
    }
        
    function isSortDirection(col, direction) {
        return listViewHelper.setSortingDirection(col, direction, $scope.options);
    }
        
    function sort(field, allow, isSystem) {
        if (allow) {
            listViewHelper.setSorting(field, allow, $scope.options);
            //console.log($scope);
            //$scope.getContent($scope.contentId);
        }
    }

    function toggleBulkActions() {
        //console.log(document.getElementById('bulkActions'));
        //console.log(vm.selection);
        document.getElementById('bulkActions').style.display = vm.selection.length > 0 ? null : "none";
    }
    
    function reportSpam() {
        var confirm = window.confirm('Are you sure you wish to report the selected comments as spam? This action will also delete the comments and cannot be reversed.');
        if (confirm) {
            var ids = [];
            for (var i = 0; i < vm.selection.length; i++) {
                ids.push(vm.selection[i].id);
            }
            $http({
                method: 'POST',
                url: '/Umbraco/backoffice/Api/AkismetApi/ReportSpam?id=' + ids.join(),
                cache: false
            }).then(function (data) {
                changePage(1);
            });
        }
    }

    function deleteComments() {
        var confirm = window.confirm('Are you sure you wish to delete the selected comments? This action cannot be reversed.');
        if (confirm) {
            var ids = [];
            for (var i = 0; i < vm.selection.length; i++) {
                ids.push(vm.selection[i].id);
            }
            $http({
                method: 'DELETE',
                url: '/Umbraco/backoffice/Api/AkismetApi/DeleteComment?id=' + ids.join(),
                cache: false
            }).then(function (data) {
                changePage(1);
            });
        }
    }
});

angular.module("umbraco").controller("AkismetSpamQueueController", function ($scope, $http, notificationsService, listViewHelper) {
    var vm = this;
    vm.page = {
        name: "Spam Queue"
    };
    vm.pagination = {
        pageNumber: 1,
        totalPages: 1
    };
    vm.options = {
        includeProperties: [
            { alias: "date", header: "Date", allowSorting: false },
            { alias: "comment", header: "Comment", allowSorting: false },
            { alias: "ip", header: "User's IP", allowSorting: false }
        ]
    };
    vm.items = [];
    vm.selection = [];
    $scope.options = {
        filter: '',
        orderBy: "date",
        orderDirection: "asc",
        bulkActionsAllowed: true
    };
    vm.deleteState = "";
    var date = new Intl.DateTimeFormat('en-US', { dateStyle: "short", timeStyle: "short" });

    var init = function () {
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetSpamComments',
            cache: false
        }).then(function (data) {
            //console.log(data);
            for (var i = 0; i < data.data.length; i++) {
                var d = data.data[i];
                vm.items.push({
                    "name": d.UserName,
                    "date": date.format(new Date(d.CommentDate)),
                    "comment": d.CommentText,
                    "ip": d.UserIp,
                    "id": d.Id
                })
            }
        });
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/AkismetApi/GetSpamCommentPageCount',
            cache: false
        }).then(function (data) {
            var pages = data.data;
            if (pages == 0)
                pages = 1;
            vm.pagination.totalPages = pages;
        });
    };

    angular.element(function () {
        init();
    });

    vm.selectItem = selectItem;
    vm.clickItem = clickItem;
    vm.selectAll = selectAll;
    vm.isSelectedAll = isSelectedAll;
    vm.isSortDirection = isSortDirection;
    vm.sort = sort;
    vm.changePage = changePage;
    vm.allowSelectAll = true;

    function changePage(pageNumber) {
        if (pageNumber != undefined) {
            //console.log(pageNumber);
            vm.items = [];
            $http({
                method: 'GET',
                url: '/Umbraco/backoffice/Api/AkismetApi/GetSpamComments?page=' + pageNumber,
                cache: false
            }).then(function (data) {
                //console.log(data);
                for (var i = 0; i < data.data.length; i++) {
                    var d = data.data[i];
                    vm.items.push({
                        "name": d.UserName,
                        "date": date.format(new Date(d.CommentDate)),
                        "comment": d.CommentText,
                        "ip": d.UserIp,
                        "id": d.Id
                    })
                }
            });
        }
    }
 
    function selectAll($event) {
        listViewHelper.selectAllItemsToggle(vm.items, vm.selection);
        toggleBulkActions();
    }
 
    function isSelectedAll() {
        return listViewHelper.isSelectedAll(vm.items, vm.selection);
    }
 
    function clickItem(item) {
        //alert("click node");
    }
 
    function selectItem(selectedItem, $index, $event) {
        //console.log(vm.selection);
        listViewHelper.selectHandler(selectedItem, $index, vm.items, vm.selection, $event);
        toggleBulkActions();
    }
        
    function isSortDirection(col, direction) {
        return listViewHelper.setSortingDirection(col, direction, $scope.options);
    }
        
    function sort(field, allow, isSystem) {
        if (allow) {
            listViewHelper.setSorting(field, allow, $scope.options);
            //console.log($scope);
            //$scope.getContent($scope.contentId);
        }
    }

    function toggleBulkActions() {
        console.log(document.getElementById('bulkActions'));
        console.log(vm.selection);
        document.getElementById('bulkActions').style.display = vm.selection.length > 0 ? null : "none";
    }

    function reportHam() {
        var confirm = window.confirm('Are you sure you wish to report the selected comments as ham? This action will move the comments out of the spam queue.');
        if (confirm) {
            var ids = [];
            for (var i = 0; i < vm.selection.length; i++) {
                ids.push(vm.selection[i].id);
            }
            $http({
                method: 'POST',
                url: '/Umbraco/backoffice/Api/AkismetApi/ReportHam?id=' + ids.join(),
                cache: false
            }).then(function (data) {
                changePage(1);
            });
        }
    }

    function deleteComments() {
        var confirm = window.confirm('Are you sure you wish to delete the selected comments? This action cannot be reversed.');
        if (confirm) {
            var ids = [];
            for (var i = 0; i < vm.selection.length; i++) {
                ids.push(vm.selection[i].id);
            }
            $http({
                method: 'DELETE',
                url: '/Umbraco/backoffice/Api/AkismetApi/DeleteComment?id=' + ids.join(),
                cache: false
            }).then(function (data) {
                changePage(1);
            });
        }
    }
});