
//jspre just gets vomited into .framework.js, right at the beginning of function unityFramework()
//if we want to slap things onto the Module object, var Module should be in scope

//Module.canvas references the canvas

//AFAICT no way to get unity framework instance from canvas object in JS
//could attach with .data instead of setting a varaible as in example maybe?

//public functions (JS->CS)
//TODO these need to be attached to Module to be accessible, or thrown into the global scope with window.whatever
Module.pushBroadcastMessage = function(flag, values) {
	callCS('PushBroadcastMessage', null, [flag], values);
}

Module.callScript = function(script, ...args) {
	callCS('CallScript', script, args);
}

function callCS (callType, target, args, namedArgs) {
	const obj = {
		callType: callType,
		target: target,		
		args: args,
		namedArgs: namedArgs
	};
	const str = JSON.stringify(obj);
	debugger;
	unityInstance.SendMessage('CCMonoBehaviourHook', 'JSCallThunk', str);
}

//TODO add event listener to Module.canvas

//functions called from CS
function callJSFunction(functionName, args) {
	debugger;
	eval(functionName)(...args); //gross
}

function triggerCanvasEvent(eventName, args) {
	debugger;
	Module.canvas.dispatchEvent(new Event(eventName, args));
}

debugger;