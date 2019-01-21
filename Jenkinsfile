echo 'Hello from Plank Juna Zone Cloud Backend'

node ('windows-only') {
    git url: 'ssh://git@git.plank.life:5443/juna/zone-backend-restapi.git'
	def workSpace = 'e:\\ws_backend'
	// Using custom workspace as windows slave does not supports file path more than 260 chars
	ws(workSpace){
		stage 'Checkout from Git repository'
		echo '*************** source code checkout complete ***************'

		def branchName = "${env.BRANCH_NAME}"
		
		if (branchName.startsWith('feature/')) {
			ws("$workSpace\\dev") {
				checkout scm
				build()
				sonarQubeAnalysis()
			}
		} else if (branchName.startsWith('develop')) {
			ws("$workSpace\\dev") {
				checkout scm
				build()
				sonarQubeAnalysis()
				uploadDevtoNexus()
				uploadDevNewsFeedtoNexus()
			}
		} else if (branchName.startsWith('release/')) {
			ws("$workSpace\\realease") {
				checkout scm
				buildReleaseBranch()
				sonarQubeAnalysis()
				uploadReltoNexus()
				uploadRelNewsFeedtoNexus()
			}
		} else if (branchName.startsWith('bugfix/')) {
			ws("$workSpace\\dev") {
				checkout scm
				build()
				sonarQubeAnalysis()
			}
		} else if (branchName.startsWith('hotfix/')) {
			ws("$workSpace\\release") {
				checkout scm
				buildReleaseBranch()
				sonarQubeAnalysis()
				uploadReltoNexus()
			}
		} else {
			error "Don't know what to do with this branch..."
		}
	}
	// TBD: To build master branch
}

// ################## Branch specific methods with task details ######################

def build() {
    echo '----------Build ----------'
	stage 'Restore Dependencies'
	bat 'nuget restore'
	
	stage 'Clean Backend Dev'
	bat 'msbuild zone-backend-restapi.sln /t:Clean /p:Configuration=Debug'
	
	stage 'Build Backend Dev'
	bat 'msbuild zone-backend-restapi.sln /t:Build /p:Configuration=Debug'
	
	stage 'Publish backend Dev'
	bat 'msbuild zone-backend-restapi.sln /t:Publish /p:Configuration=Debug'
}

//def executeTests(){
//	stage 'Unit Test'
//	  dotnet vstest TestZoneService.UnitTest\\bin\\Debug\\TestZoneService.UnitTest.dll
//	stage 'Integration Test'
//	bat 'vstest.console TestZoneRestApi\\bin\\Debug\\TestZoneRestApi.dll'
//}

def	sonarQubeAnalysis() {
   stage 'Code Quality Analysis'
	bat 'SonarScanner.MSBuild.exe begin /k:"org.sonarqube:sonarqube-scanner-msbuild" /n:"Zone-backend-rest-api" /v:"1.0"'
	bat 'msbuild /t:Rebuild /p:Configuration=Debug'
	bat 'SonarScanner.MSBuild.exe end'
}

def buildReleaseBranch() {
	echo '----------Build Staging----------'
   	 stage 'Clean Backend Release'
	bat 'msbuild zone-backend-restapi.sln /t:Clean /p:Configuration=Release'
	stage 'Build Backend Release'
	bat 'msbuild zone-backend-restapi.sln /t:Build /p:Configuration=Release'
	stage 'Publish backend Release'
	bat 'msbuild zone-backend-restapi.sln /t:Publish /p:Configuration=Release'
}


def uploadDevtoNexus() {
   stage 'Zip and Upload Artifacts to Nexus' 
   dir('Juna.Zone.Feed.WebApi\\bin\\Debug\\netcoreapp2.1\\'){
            env.IMAGE_NAME = "backendArtifacts-" + env.BUILD_ID + '.zip'
           	zip dir: '\\publish',  glob: ' ',  zipFile: env.IMAGE_NAME
			archiveArtifacts  env.IMAGE_NAME
			bat "curl.exe -v -u admin:admin123 --upload-file ${env.IMAGE_NAME}  http://artifactory.plank.life:8081/repository/snapshots/"
			if (fileExists(env.IMAGE_NAME)) {
           bat "del ${env.IMAGE_NAME}"
		  } 
		 deleteDir()
	}
}

def uploadDevNewsFeedtoNexus() {
	stage 'Zip and Upload FeedFlows Artifacts to Nexus'
	dir('Juna.Zone.FeedFlows\\bin\\Debug\\netstandard2.0\\'){
			env.IMAGE_NAME = "backendNewsFeedAggregator-" + env.BUILD_ID + '.zip'
			zip dir: '\\bin',  glob: ' ',  zipFile: env.IMAGE_NAME
			archiveArtifacts  env.IMAGE_NAME
			bat "curl.exe -v -u admin:admin123 --upload-file ${env.IMAGE_NAME}  http://artifactory.plank.life:8081/repository/snapshots/"
			if (fileExists(env.IMAGE_NAME)) {
		   bat "del ${env.IMAGE_NAME}"
		  }
		 deleteDir()
	}
}

def uploadReltoNexus() {
   stage 'Zip and Upload Artifacts to Nexus' 
   dir('Juna.Zone.Feed.WebApi\\bin\\Debug\\netcoreapp2.1\\'){
            env.IMAGE_NAME = "backendArtifacts-" + '1.0.0' + '.zip'
           	zip dir: '\\publish',  glob: ' ',  zipFile: env.IMAGE_NAME
			archiveArtifacts  env.IMAGE_NAME
			bat "curl.exe -v -u admin:admin123 --upload-file ${env.IMAGE_NAME}  http://artifactory.plank.life:8081/repository/releases/"
			if (fileExists(env.IMAGE_NAME)) {
           bat "del ${env.IMAGE_NAME}"
		  } 
		 deleteDir()
	}
}

def uploadRelNewsFeedtoNexus() {
	stage 'Zip and Upload FeedFlows Artifacts to Nexus'
	dir('Juna.Zone.FeedFlows\\bin\\Debug\\netstandard2.0\\'){
			env.IMAGE_NAME = "backendNewsFeedAggregator-" + '1.0.0' + '.zip'
			zip dir: '\\bin',  glob: ' ',  zipFile: env.IMAGE_NAME
			archiveArtifacts  env.IMAGE_NAME
			bat "curl.exe -v -u admin:admin123 --upload-file ${env.IMAGE_NAME}  http://artifactory.plank.life:8081/repository/releases/"
			if (fileExists(env.IMAGE_NAME)) {
		   bat "del ${env.IMAGE_NAME}"
		  }
		 deleteDir()
	}
}
