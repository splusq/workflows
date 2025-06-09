const { exec } = require('child_process');
const util = require('util');
const execPromise = util.promisify(exec);

async function runProcess(command) {
    try {
        const { stdout, stderr } = await execPromise(command);
        return "Bearer " + stdout.replace(/\r?\n|\r/g, '');
    } catch (error) {
        return "";
    }
}

async function getAccessToken() {
    return await runProcess('az account get-access-token --scope "https://ai.azure.com/.default" --query accessToken -o tsv')
}

module.exports = {
    getAccessToken
};

