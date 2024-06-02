import { Button } from "./components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "./components/ui/card";
import { Label } from "./components/ui/label";
import { Input } from "./components/ui/input";
import {
  CredentialResponse,
  GoogleLogin,
  useGoogleLogin,
  useGoogleOneTapLogin,
} from "@react-oauth/google";
import { FaGoogle } from "react-icons/fa";
import axios from "axios";
import { Separator } from "./components/ui/separator";
import { Checkbox } from "./components/ui/checkbox";

const LoginForm = () => {
  const responseMessage = (response: CredentialResponse | undefined) => {
    console.log(response);
    if (!response) return;

    let tokenId = response.credential;
    try {
      axios
        .post("https://localhost:7278/api/auth/signin-google", {
          tokenId,
        })
        .then((res) => {
          console.log(res.data.token);

          axios
            .get("https://localhost:7278/api/auth/auth-required", {
              headers: {
                Authorization: `Bearer ${res.data.token}`,
              },
            })
            .then((res) => console.log(res.data));
        });
    } catch (error) {
      console.error(error);
    }
  };
  const login = useGoogleOneTapLogin({
    onSuccess: (res) => console.log(res),
  });
  return (
    <Card className="w-[350px] shadow-2xl">
      <CardHeader>
        <CardTitle className="mb-3">GeoTech Hours</CardTitle>
        <CardDescription>Hey there! Welcome back</CardDescription>
        <GoogleLogin
          logo_alignment="center"
          width={"300"}
          onSuccess={(credential: CredentialResponse) =>
            responseMessage(credential)
          }
          onError={() => console.log("An error occurred during Google login")}
        />
        {/* <Button variant={"outline"} onClick={login}>
          <FaGoogle className="text-lg font-bold mr-3" /> Sign in with Google
        </Button> */}
        <span className="flex justify-center items-center">
          <Separator className="w-28" />{" "}
          <span className="mx-7 text-muted-foreground">or</span>{" "}
          <Separator className="w-28" />
        </span>
      </CardHeader>
      <CardContent>
        <form>
          <div className="grid w-full items-center gap-4">
            <div className="flex flex-col space-y-1.5">
              <Input id="emailId" placeholder="Enter your email" />
            </div>
            <div className="flex flex-col space-y-1.5">
              <Input id="password" placeholder="Enter your password" />
            </div>
          </div>
        </form>
        <div className="my-2 flex justify-between items-center">
          <span className="flex gap-1.5 leading-none">
            <Checkbox />
            <Label>Remember Me</Label>
          </span>
          <span>
            <Button variant={"link"}>Forgot Password?</Button>
          </span>
        </div>
      </CardContent>
      <CardFooter>
        <Button className="w-full">Login</Button>
      </CardFooter>
    </Card>
  );
};

export default LoginForm;
