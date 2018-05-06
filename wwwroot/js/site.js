$(document).ready(function () {

    var selectedTweets = (function () {

        var key = "selected-tweets";

        var selectedTweets = [];

        if (sessionStorage.getItem(key)) {
            selectedTweets = JSON.parse(sessionStorage.getItem(key));
        }
        else {
            sessionStorage.setItem(key, JSON.stringify(selectedTweets));
        }

        function getSelectedTweets() {
            return JSON.parse(sessionStorage.getItem(key));
        }

        function pushTweet(tweetId) {

            selectedTweets.push(tweetId);
            sessionStorage.setItem(key, JSON.stringify(selectedTweets));
        }

        function removeTweet(tweetId) {

            var index = selectedTweets.indexOf(tweetId);

            if (index > -1) {
                selectedTweets.splice(index, 1);
            }
            
            sessionStorage.setItem(key, JSON.stringify(selectedTweets));
        }

        function contains(tweetId) {

            return selectedTweets.includes(tweetId);
        }

        function clear() {

            selectedTweets = [];
            sessionStorage.setItem(key, JSON.stringify(selectedTweets));
        }

        return {
            get: getSelectedTweets,
            push: pushTweet,
            remove: removeTweet,
            contains: contains,
            clear: clear
        }

    })();

    $(".tweet-checkbox").each(function () {
        var id = this.value;

        if (selectedTweets.contains(id)) {
            this.checked = true;
        }
    });
    
    $(".tweet-checkbox").change(function (event) {
       
        var selected = this.checked;

        if (selected) {
            selectedTweets.push(this.value);
        }
        else {
            selectedTweets.remove(this.value);
        }
    });

    function deselectChecked() {
        $(".tweet-checkbox").each(function () {

            if (this.checked) {
                this.checked = false;
            }
        });
    }

    $("#save-to-file").click(function () {

        var selectedTweetsIds = selectedTweets.get();

        if (selectedTweetsIds.length > 0) {
            $.post("/tweet/save/file", { selectedTweetIds: selectedTweetsIds }, function () {

                deselectChecked();

                selectedTweets.clear();
                alert("Successfully saved!");

            }).fail(function (response) {

                alert('Error: something went wrong! Please, try again.');
            });
        }
        else {
            alert('Please, select some tweets!')
        }
    });
});