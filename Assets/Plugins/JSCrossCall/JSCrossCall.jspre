//public functions (JS->CS)
Module.pushBroadcastMessage = function(flag, values) {
	callCS('PushBroadcastMessage', null, [flag], values);
}

Module.callScript = function(script, ...args) {
	callCS('CallScript', script, args);
}

//internal functions

function callCS (callType, target, args, namedArgs) {
	const obj = {
		callType: callType,
		target: target,		
		args: args,
		namedArgs: namedArgs
	};
	const str = JSON.stringify(obj);
	Module.SendMessage('CCMonoBehaviourHook', 'JSCallThunk', str);
}

//functions called from CS
function callJSFunction(functionName, args) {
	eval(functionName)(...args); //gross
}

function triggerCanvasEvent(eventName, args) {
	Module.canvas.dispatchEvent(new Event(eventName, args));
}
