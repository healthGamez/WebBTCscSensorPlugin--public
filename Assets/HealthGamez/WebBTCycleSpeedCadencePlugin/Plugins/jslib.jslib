mergeInto(LibraryManager.library,
    {
		BeConnect: function () {
			connect();
		},
        BeIsConnected: function () {
            return isConnected;
        },

    });