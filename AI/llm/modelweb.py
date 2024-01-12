
from flask import Flask, request, render_template
import base64
import openai
import os

app = Flask(__name__)
openai.api_key = os.getenv("OPENAI_API_KEY")

@app.route('/', methods=['GET', 'POST'])
def upload_file():
    if request.method == 'POST':
        old_images = request.files.getlist('old_image_upload')
        new_images = request.files.getlist('new_image_upload')

        print(len(old_images))
        for image in old_images:
            print(image)
	    
        encoded_old_images = [encode_image_to_base64(image) for image in old_images]
        encoded_new_images = [encode_image_to_base64(image) for image in new_images]

        # Prepare and send OpenAI API request here
        response = send_openai_request(encoded_old_images, encoded_new_images)
        return response
    return render_template('form.html')

def encode_image_to_base64(image_file):
    image_file.seek(0)  # Reset file pointer to the beginning
    return base64.b64encode(image_file.read()).decode('utf-8')

def send_openai_request(encoded_old_images, encoded_new_images):
    system_prompt = """You are required to provide feedback on my progress on a given goal. I will provide you with screenshots from all my screens, and you will respond with feedback according to the guidelines provided below. You must follow the following steps:

	FEEDBACK_GUIDELINES: You will get the guidelines to provide feedback on my progress.
	GET_GOAL: The user will provide you with the current goal to achieve.
	GIVE_REFLECTION: Reflect on the perceived activities performed by the user based on the desired goal and the attached images.
	GIVE_GENERAL_FEEDBACK: You will provide feedback on how the perceived activities on the screens advance to my goal.
	GIVE_ACTIVITIES_FEEDBACK: You will provide some recommendations on next activities that I should perform.
	GIVE_POSITIONING_FEEDBACK: You will provide feedback on how I can better distribute the windows on the different screens.

	Begin!

	FEEDBACK_GUIDELINES: You will find image files attached to this prompt. The files that start with old_XX.png means that are screenshots taken two minutes ago. The files that start with new_XX.png mean that are the latest screenshots. The XX wildcard represents the number of the monitor to which the screenshots belong. Notice that if no old_XX.png images are provided, it means that this is the first time the prompt is executed. Based on the images you get, you are required to: 1) reflect on what activities has been performed based on the screenshots; 2) identify if the activities are aligned to the current goal; 3) propose activities to be performed by the user to achieve the goal; and 4) you will provide suggestions on how to arrange the windows on the different screens. It is important that all the pieces of feedback provided are brief, no more than three sentences each. Be concise.
	GET_GOAL:
	""" # Your existing system prompt
   # goal = "I want to finish my homework on important people in tech"
    goal =""
    prompt = system_prompt + goal + "\nGIVE_REFLECTION: "

    messages = [{"role": "user", "content": prompt}]

    for image in encoded_old_images:
        messages.append({
            "role": "user",
            "content": f"data:image/jpeg;base64,{image}"
        })

    for image in encoded_new_images:
        messages.append({
            "role": "user",
            "content": f"data:image/jpeg;base64,{image}"
        })

    response = openai.ChatCompletion.create(
        model="gpt-4-vision-preview",
        messages=messages,
        max_tokens=300,
    )
    return str(response.choices[0])


if __name__ == '__main__':
    app.run(debug=True)
































