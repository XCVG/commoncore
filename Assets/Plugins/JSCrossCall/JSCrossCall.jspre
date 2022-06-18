
//jspre just gets vomited into .framework.js, right at the beginning of function unityFramework()
//if we want to slap things onto the Module object, var Module should be in scope

//Module.canvas references the canvas

//AFAICT no way to get unity framework instance from canvas object in JS
//could attach with .data instead of setting a varaible as in example maybe?

//public functions (JS->CS)
//TODO these need to be attached to Module to be accessible, or thrown into the global scope with window.whatever
function pushBroadcastMessage(flag, values) {
	
}

function callScript (script, ...args) {
	
}

debugger;