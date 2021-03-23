angular.module("umbraco").controller("AkismetController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.loading = true;

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
            } else {
                acct.classList.add("alert", "alert-error");
                acct.innerText = "There was a problem loading account details";
            }
        });
    };
});

angular.module("umbraco").controller("AkismetConfigController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.page = {};
    vm.page.name = "Configuration";
    //console.log(vm);
    console.log($scope);

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
            })
                .then(function (data) {
                    // data: data, status, headers, config

                    if (data.data) {
                        vm.buttonState = "success";
                        notificationsService.success("API key saved");
                    } else {
                        vm.buttonState = "error";
                        notificationsService.error("API key invalid");
                    }
                });
        }
    }

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