mergeInto(LibraryManager.library, {

	//this is referenced from CS
	CSCallThunk: function (strPtr) {
		const dataStr = Pointer_stringify(strPtr);
		const data = JSON.parse(dataStr);

		if(data.functionName) {
			callJSFunction(data.functionName, data.args);
		}
		else if(data.eventName) {
			triggerCanvasEvent(data.eventName, data.args)
		}
	}
});
