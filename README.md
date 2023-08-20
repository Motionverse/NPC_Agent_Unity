## Overview
Users can freely move around the village and have conversations with each NPC character. The NPC characters will respond according to their assigned personas. We have integrated Microsoft's speech recognition and large language models, and directly used the GPT3.5 API. With the generated text responses, we can leverage the powerful Motionverse platform to set animation styles, voice tones, and enable the NPCs to easily complete speaking animations.

You can watch the demo video for our project.
[![Unity NPC Agent](https://res.cloudinary.com/marcomontalbano/image/upload/v1692500688/video_to_markdown/images/youtube--Vk5Iq8yMIBQ-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/Vk5Iq8yMIBQ "Unity NPC Agent")

## Usage

Open NPC_Agent_Unity\Assets\Scenes\GameScene.unity

1、Firstly, configure the Microsoft Azure account, which is mainly used for speech recognition.
The related IDs can be obtained on the website.  
 <br />https://speech.microsoft.com/portal

![image](https://github.com/Motionverse/NPC_Agent_Unity/assets/109574037/c33884a2-d2e8-40f0-9618-2d8112b3913f)


You can refer to Microsoft's documentation for the parameters， For example, en-US,de-DE,zh-CN
 <br />https://markdown.com.cn](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=stt


2、The second step, you need to have a chatgpt key, which can be obtained on the OpenAI website.It usually starts with ‘sk’.
![image](https://github.com/Motionverse/NPC_Agent_Unity/assets/109574037/e3618613-b3cf-48ee-a383-b3872a13f033)

3、The third step is to obtain the APPID and KEY of the Motionverse platform 
Our Motionverse is mainly used to generate actions and expressions.
 <br />Asia region please visit： [motionverse.deepscience.cn](https://motionverse.deepscience.cn/#/)
 <br />Other regions please visit：[motionverse.ai](https://motionverse.ai/)


![image](https://github.com/Motionverse/NPC_Agent_Unity/assets/109574037/39868509-f432-4dfc-9589-f517a8cf6257)

Now you can run normally！

We can also configure the character personality and vocal tone for each role.
![image](https://github.com/Motionverse/NPC_Agent_Unity/assets/109574037/8509bf2a-bf99-416e-9ba4-d66c36c46aa0)
![image](https://github.com/Motionverse/NPC_Agent_Unity/assets/109574037/f1b3c65b-130d-4f1a-ace6-d07cdf09dcad)


motionverse.ai users can refer to this documentation to select voice tones.
<br /> http://doc.motionverse.ai/BOARDCAST.html#tts_voice_name



This project follows the all-contributors specification. Contributions of any kind welcome!
<br />You can contact us at support@motionverse.ai for any questions.
