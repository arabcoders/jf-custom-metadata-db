const CustomMetaDataConfig = {
    pluginUniqueId: '83b77e24-9fce-4ee0-a794-73fdfa972e66'
};
const isValidUrl = urlString => {
    try {
        return Boolean(new URL(urlString));
    }
    catch (e) {
        return false;
    }
}

export default function (view) {
    view.addEventListener('viewshow', function () {
        Dashboard.showLoadingMsg();
        const page = this;
        ApiClient.getPluginConfiguration(CustomMetaDataConfig.pluginUniqueId).then(function (config) {
            page.querySelector('#api_url').value = config.ApiUrl || '';
            page.querySelector('#api_ref_url').value = config.ApiRefUrl || '';
            Dashboard.hideLoadingMsg();
        });
    });

    view.querySelector('#CMDConfigForm').addEventListener('submit', function (e) {
        Dashboard.showLoadingMsg();
        const form = this;
        ApiClient.getPluginConfiguration(CustomMetaDataConfig.pluginUniqueId).then(function (config) {
            var apiUrl = form.querySelector('#api_url').value;
            if (false === isValidUrl(apiUrl)) {
                Dashboard.hideLoadingMsg();
                Dashboard.alert('Invalid API URL');
                form.querySelector('#api_url').focus();
                return false;
            }

            var ApiRefUrl = form.querySelector('#api_ref_url').value;

            if (false === ApiRefUrl.includes('{0}')) {
                Dashboard.hideLoadingMsg();
                Dashboard.alert('Invalid API Reference URL, the reference url must include {0}');
                form.querySelector('#api_ref_url').focus();
                return false;
            }

            config.ApiUrl = apiUrl;
            config.ApiRefUrl = ApiRefUrl;

            ApiClient.updatePluginConfiguration(CustomMetaDataConfig.pluginUniqueId, config).then(function (result) {
                Dashboard.processPluginConfigurationUpdateResult(result);
            });
        });
        e.preventDefault();
        return false;
    });
}
